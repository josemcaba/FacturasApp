const API = '/api/facturas';

let uploadedRutas = [];
let uploadedRutaExcel = null;
let allFacturas = [];

// ── DOM refs ────────────────────────────────────────────────────────
const $ = id => document.getElementById(id);
const uploadZone = $('upload-zone');
const fileInput = $('file-input');
const fileSection = $('files-section');
const fileList = $('file-list');
const fileCount = $('file-count');
const progressSection = $('progress-section');
const progressFill = $('progress-fill');
const progressText = $('progress-text');
const filtersSection = $('filters-section');
const summarySection = $('summary-section');
const resultsSection = $('results-section');
const tbody = $('facturas-tbody');
const modalOverlay = $('modal-overlay');
const modalBody = $('modal-body');

// ── Upload ──────────────────────────────────────────────────────────
uploadZone.addEventListener('click', () => fileInput.click());

uploadZone.addEventListener('dragover', e => {
    e.preventDefault();
    uploadZone.classList.add('dragover');
});

uploadZone.addEventListener('dragleave', () => {
    uploadZone.classList.remove('dragover');
});

uploadZone.addEventListener('drop', e => {
    e.preventDefault();
    uploadZone.classList.remove('dragover');
    handleFiles(e.dataTransfer.files);
});

fileInput.addEventListener('change', () => {
    handleFiles(fileInput.files);
    fileInput.value = '';
});

async function handleFiles(files) {
    const formData = new FormData();
    let hasValidFile = false;
    for (const file of files) {
        const name = file.name.toLowerCase();
        if (name.endsWith('.pdf') || name.endsWith('.xlsx') || name.endsWith('.xls')) {
            formData.append('files', file);
            hasValidFile = true;
        }
    }

    if (!hasValidFile) return;

    showProgress(0, 'Subiendo...');

    try {
        const resp = await fetch(`${API}/upload`, { method: 'POST', body: formData });
        if (!resp.ok) throw new Error(await resp.text());
        const data = await resp.json();

        uploadedRutas.push(...data.Rutas);
        if (data.RutaExcel) uploadedRutaExcel = data.RutaExcel;
        updateFileList(files);
    } catch (err) {
        alert('Error subiendo archivos: ' + err.message);
    }

    hideProgress();
}

function updateFileList(files) {
    fileSection.classList.remove('hidden');
    fileCount.textContent = uploadedRutas.length + (uploadedRutaExcel ? 1 : 0);
    for (const file of files) {
        const name = file.name.toLowerCase();
        if (name.endsWith('.pdf') || name.endsWith('.xlsx') || name.endsWith('.xls')) {
            const li = document.createElement('li');
            li.textContent = file.name;
            if (name.endsWith('.xlsx') || name.endsWith('.xls'))
                li.style.color = '#bb86fc';
            fileList.appendChild(li);
        }
    }
}

// ── Process ─────────────────────────────────────────────────────────
$('btn-process').addEventListener('click', async () => {
    if (uploadedRutas.length === 0 && !uploadedRutaExcel) return;

    showProgress(10, 'Procesando facturas...');

    try {
        const resp = await fetch(`${API}/process`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Rutas: uploadedRutas, RutaExcel: uploadedRutaExcel })
        });

        if (!resp.ok) throw new Error(await resp.text());
        const data = await resp.json();

        showProgress(80, 'Cargando resultados...');
        allFacturas.push(...data);
        renderTable(allFacturas);
        await loadSummary();

        uploadedRutas = [];
        uploadedRutaExcel = null;
        fileList.innerHTML = '';
        fileSection.classList.add('hidden');
    } catch (err) {
        alert('Error procesando: ' + err.message);
    }

    hideProgress();
});

// ── Clear ───────────────────────────────────────────────────────────
$('btn-clear-files').addEventListener('click', clearAll);
$('btn-clear').addEventListener('click', clearAll);

async function clearAll() {
    uploadedRutas = [];
    uploadedRutaExcel = null;
    fileList.innerHTML = '';
    fileSection.classList.add('hidden');

    await fetch(API, { method: 'DELETE' });
    allFacturas = [];
    tbody.innerHTML = '';
    filtersSection.classList.add('hidden');
    summarySection.classList.add('hidden');
    resultsSection.classList.add('hidden');
}

// ── Filters ─────────────────────────────────────────────────────────
$('filter-text').addEventListener('input', applyFilters);
$('filter-estado').addEventListener('change', applyFilters);

function applyFilters() {
    const texto = $('filter-text').value.toUpperCase();
    const estado = $('filter-estado').value;

    let filtered = allFacturas;

    if (texto) {
        filtered = filtered.filter(f =>
            (f.NumeroFactura || '').toUpperCase().includes(texto) ||
            (f.EmisorNombre || '').toUpperCase().includes(texto) ||
            (f.EmisorNif || '').toUpperCase().includes(texto) ||
            (f.ReceptorNombre || '').toUpperCase().includes(texto));
    }

    if (estado !== 'Todos') {
        filtered = filtered.filter(f => f.Estado === estado);
    }

    renderTable(filtered);
}

// ── Render table ────────────────────────────────────────────────────
function renderTable(facturas) {
    tbody.innerHTML = '';
    resultsSection.classList.remove('hidden');
    filtersSection.classList.remove('hidden');

    facturas.forEach((f, i) => {
        const tr = document.createElement('tr');
        tr.className = `estado-${f.Estado}`;
        tr.innerHTML = `
            <td>${f.NumeroFactura}<br><small>${f.Fecha || ''}</small></td>
            <td>${f.EmisorNombre}<br><small>${f.EmisorNif}</small></td>
            <td>${f.ReceptorNombre}<br><small>${f.ReceptorNif}</small></td>
            <td class="num">${formatEuro(f.BaseImponible)}</td>
            <td class="num">${formatEuro(f.CuotaIVA)}<br><small>${formatPct(f.PorcentajeIVA)}</small></td>
            <td class="num">${formatEuro(f.CuotaIRPF)}<br><small>${formatPct(f.PorcentajeIRPF)}</small></td>
            <td class="num">${formatEuro(f.CuotaRE)}<br><small>${formatPct(f.PorcentajeRE)}</small></td>
            <td class="num">${formatEuro(f.TotalFactura)}</td>
            <td>${f.EstadoDisplay}</td>
            <td><button class="btn btn-sm" data-idx="${i}">Ver</button></td>
        `;
        tr.querySelector('button').addEventListener('click', () => showDetail(f));
        tbody.appendChild(tr);
    });
}

// ── Summary ─────────────────────────────────────────────────────────
async function loadSummary() {
    try {
        const resp = await fetch(`${API}/resumen`);
        const data = await resp.json();

        $('sum-total').textContent = formatEntero(data.Total);
        $('sum-ok').textContent = formatEntero(data.OK);
        $('sum-revisar').textContent = formatEntero(data.Revisar);
        $('sum-error').textContent = formatEntero(data.Error);
        $('sum-duplicada').textContent = formatEntero(data.Duplicada);
        $('sum-euros').textContent = formatEuro(data.TotalEuros);

        summarySection.classList.remove('hidden');
    } catch {}
}

// ── Detail modal ────────────────────────────────────────────────────
function showDetail(f) {
    modalBody.innerHTML = `
        <div class="detail-group">
            <h3>FACTURA</h3>
            <div class="detail-row"><span class="label">Numero</span><span class="value">${f.NumeroFactura}</span></div>
            <div class="detail-row"><span class="label">Fecha</span><span class="value">${f.Fecha || '-'}</span></div>
            <div class="detail-row"><span class="label">Estado</span><span class="value">${f.EstadoDisplay}</span></div>
            <div class="detail-row"><span class="label">OCR</span><span class="value">${f.ExtractedByOcr ? 'Si' : 'No'}</span></div>
        </div>
        <div class="detail-group">
            <h3>EMISOR</h3>
            <div class="detail-row"><span class="label">Nombre</span><span class="value">${f.EmisorNombre}</span></div>
            <div class="detail-row"><span class="label">NIF</span><span class="value">${f.EmisorNif}</span></div>
        </div>
        <div class="detail-group">
            <h3>RECEPTOR</h3>
            <div class="detail-row"><span class="label">Nombre</span><span class="value">${f.ReceptorNombre}</span></div>
            <div class="detail-row"><span class="label">NIF</span><span class="value">${f.ReceptorNif}</span></div>
        </div>
        <div class="detail-group">
            <h3>IMPORTES</h3>
            <div class="detail-row"><span class="label">Base imponible</span><span class="value">${formatEuro(f.BaseImponible)}</span></div>
            <div class="detail-row"><span class="label">IVA (${formatPct(f.PorcentajeIVA)})</span><span class="value">${formatEuro(f.CuotaIVA)}</span></div>
            <div class="detail-row"><span class="label">IRPF (${formatPct(f.PorcentajeIRPF)})</span><span class="value">${formatEuro(f.CuotaIRPF)}</span></div>
            <div class="detail-row"><span class="label">RE (${formatPct(f.PorcentajeRE)})</span><span class="value">${formatEuro(f.CuotaRE)}</span></div>
            <div class="detail-row"><span class="label">Total factura</span><span class="value">${formatEuro(f.TotalFactura)}</span></div>
            <div class="detail-row"><span class="label">Total calculado</span><span class="value">${formatEuro(f.TotalCalculado)}</span></div>
        </div>
        ${(f.MensajesError && f.MensajesError.length > 0) ? `
        <div class="detail-group">
            <h3>MENSAJES</h3>
            <div class="messages">${f.MensajesError.map(m => `<p>${m}</p>`).join('')}</div>
        </div>` : ''}
    `;
    modalOverlay.classList.remove('hidden');
}

$('modal-close').addEventListener('click', () => modalOverlay.classList.add('hidden'));
modalOverlay.addEventListener('click', e => {
    if (e.target === modalOverlay) modalOverlay.classList.add('hidden');
});

// ── Export ──────────────────────────────────────────────────────────
$('btn-export-ingresos').addEventListener('click', () => downloadExcel('ingresos'));
$('btn-export-gastos').addEventListener('click', () => downloadExcel('gastos'));

function downloadExcel(tipo) {
    const a = document.createElement('a');
    a.href = `${API}/export/${tipo}`;
    a.download = tipo === 'gastos' ? 'Gastos_FacturasApp.xlsx' : 'Ingresos_FacturasApp.xlsx';
    a.click();
}

// ── Helpers ─────────────────────────────────────────────────────────
function formatEuro(valor) {
    if (valor == null) return '-';
    return new Intl.NumberFormat('es-ES', { style: 'decimal', useGrouping: true, minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Number(valor)) + ' EUR';
}

function formatPct(valor) {
    if (valor == null) return '-';
    return new Intl.NumberFormat('es-ES', { style: 'decimal', useGrouping: true, minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(Number(valor)) + '%';
}

function formatEntero(valor) {
    if (valor == null) return '-';
    return new Intl.NumberFormat('es-ES', { style: 'decimal', useGrouping: true }).format(Number(valor));
}

function showProgress(pct, text) {
    progressSection.classList.remove('hidden');
    progressFill.style.width = pct + '%';
    progressText.textContent = text || '';
}

function hideProgress() {
    progressSection.classList.add('hidden');
    progressFill.style.width = '0%';
}
