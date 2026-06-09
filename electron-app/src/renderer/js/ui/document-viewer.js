const documentViewer = {
  placeholderEl: document.getElementById('content-placeholder'),
  detailEl: document.getElementById('content-detail'),
  titleEl: document.getElementById('detail-title'),
  metaEl: document.getElementById('detail-meta'),
  tagsEl: document.getElementById('detail-tags'),
  bodyEl: document.getElementById('detail-body'),
  actionsEl: document.getElementById('detail-actions'),
  historyPanel: document.getElementById('history-panel'),
  auditPanel: document.getElementById('audit-panel'),
  versionListEl: document.getElementById('version-list'),
  auditListEl: document.getElementById('audit-list'),

  showLoading() {
    this.placeholderEl.classList.add('hidden');
    this.detailEl.classList.add('hidden');
    this.historyPanel.classList.add('hidden');
    this.auditPanel.classList.add('hidden');
  },

  showError() {
    this.placeholderEl.classList.add('hidden');
    this.detailEl.classList.remove('hidden');
    this.titleEl.textContent = 'Error';
    this.metaEl.textContent = 'No se pudo cargar el documento.';
    this.bodyEl.innerHTML = '<p>Verifica la conexión con la API.</p>';
  },

  render(doc) {
    this.placeholderEl.classList.add('hidden');
    this.detailEl.classList.remove('hidden');
    this.historyPanel.classList.add('hidden');
    this.auditPanel.classList.add('hidden');

    this.titleEl.textContent = doc.title;

    const parts = [];
    if (doc.status) parts.push(`Estado: ${doc.status}`);
    if (doc.currentVersion) parts.push(`Versión: ${doc.currentVersion}`);
    if (doc.updatedAt) parts.push(`Actualizado: ${new Date(doc.updatedAt).toLocaleString()}`);
    this.metaEl.textContent = parts.join(' · ');

    if (doc.tags && doc.tags.length > 0) {
      this.tagsEl.innerHTML = doc.tags.map(t => `<span class="tag">${t}</span>`).join('');
      this.tagsEl.classList.remove('hidden');
    } else {
      this.tagsEl.innerHTML = '';
      this.tagsEl.classList.add('hidden');
    }

    this.bodyEl.innerHTML = doc.renderedHtml || '<p><em>Sin contenido.</em></p>';

    this._currentDocId = doc.id;
    this.actionsEl.classList.remove('hidden');
  },

  async showVersions() {
    if (!this._currentDocId) return;
    this.detailEl.classList.add('hidden');
    this.historyPanel.classList.remove('hidden');
    this.versionListEl.innerHTML = '<p class="state-msg">Cargando…</p>';

    try {
      const versions = await api.getVersions(this._currentDocId);
      if (versions.length === 0) {
        this.versionListEl.innerHTML = '<p class="state-msg">Sin versiones.</p>';
        return;
      }
      this.versionListEl.innerHTML = versions.map(v => `
        <div class="version-item ${v.isCurrent ? 'current' : ''}">
          <strong>v${v.versionNumber}</strong>
          ${v.isCurrent ? '<span class="badge badge-ok">Actual</span>' : ''}
          <p>${v.changeSummary || 'Sin descripción'}</p>
          <small>${v.createdAt ? new Date(v.createdAt).toLocaleString() : ''} ${v.createdBy ? `por ${v.createdBy}` : ''}</small>
        </div>
      `).join('');
    } catch {
      this.versionListEl.innerHTML = '<p class="state-msg">Error al cargar versiones.</p>';
    }
  },

  async showAudit() {
    if (!this._currentDocId) return;
    this.detailEl.classList.add('hidden');
    this.auditPanel.classList.remove('hidden');
    this.auditListEl.innerHTML = '<p class="state-msg">Cargando…</p>';

    try {
      const audits = await api.getAudit(this._currentDocId);
      if (audits.length === 0) {
        this.auditListEl.innerHTML = '<p class="state-msg">Sin registros de auditoría.</p>';
        return;
      }
      this.auditListEl.innerHTML = audits.map(a => `
        <div class="audit-item">
          <strong>${a.action}</strong>
          <small>${a.timestamp ? new Date(a.timestamp).toLocaleString() : ''} ${a.performedBy ? `por ${a.performedBy}` : ''}</small>
          ${a.changesJson ? `<pre class="audit-changes">${a.changesJson}</pre>` : ''}
        </div>
      `).join('');
    } catch {
      this.auditListEl.innerHTML = '<p class="state-msg">Error al cargar auditoría.</p>';
    }
  },

  backToDetail() {
    this.historyPanel.classList.add('hidden');
    this.auditPanel.classList.add('hidden');
    this.detailEl.classList.remove('hidden');
  },

  showEmpty() {
    this.placeholderEl.classList.remove('hidden');
    this.detailEl.classList.add('hidden');
    this.historyPanel.classList.add('hidden');
    this.auditPanel.classList.add('hidden');
  }
};

document.getElementById('btn-versions').addEventListener('click', () => documentViewer.showVersions());
document.getElementById('btn-audit').addEventListener('click', () => documentViewer.showAudit());
document.getElementById('btn-back-detail').addEventListener('click', () => documentViewer.backToDetail());
document.getElementById('btn-back-audit').addEventListener('click', () => documentViewer.backToDetail());
