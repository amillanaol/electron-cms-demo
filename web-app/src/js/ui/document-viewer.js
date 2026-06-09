const documentViewer = {
  placeholderEl: document.getElementById('content-placeholder'),
  detailEl: document.getElementById('content-detail'),
  titleEl: document.getElementById('detail-title'),
  metaEl: document.getElementById('detail-meta'),
  bodyEl: document.getElementById('detail-body'),

  showLoading() {
    this.placeholderEl.classList.add('hidden');
    this.detailEl.classList.add('hidden');
  },

  showError() {
    this.placeholderEl.classList.add('hidden');
    this.detailEl.classList.remove('hidden');
    this.titleEl.textContent = 'Error';
    this.metaEl.textContent = 'No se pudo cargar el documento.';
    this.bodyEl.innerHTML = '<p>Verifica la conexion con la API.</p>';
  },

  render(doc) {
    this.placeholderEl.classList.add('hidden');
    this.detailEl.classList.remove('hidden');
    this.titleEl.textContent = doc.title;

    const parts = [];
    if (doc.status) parts.push(`Estado: ${doc.status}`);
    if (doc.updatedAt) parts.push(`Actualizado: ${new Date(doc.updatedAt).toLocaleString()}`);
    this.metaEl.textContent = parts.join(' · ');

    this.bodyEl.innerHTML = doc.renderedHtml || '<p><em>Sin contenido.</em></p>';
  },

  showEmpty() {
    this.placeholderEl.classList.remove('hidden');
    this.detailEl.classList.add('hidden');
  },
};
