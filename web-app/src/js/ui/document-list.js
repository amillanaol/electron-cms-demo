const documentList = {
  listEl: document.getElementById('document-list'),
  loadingEl: document.getElementById('loading-msg'),
  emptyEl: document.getElementById('empty-msg'),
  errorEl: document.getElementById('error-msg'),

  showLoading() {
    this.listEl.innerHTML = '';
    this.loadingEl.classList.remove('hidden');
    this.emptyEl.classList.add('hidden');
    this.errorEl.classList.add('hidden');
  },

  showError() {
    this.listEl.innerHTML = '';
    this.loadingEl.classList.add('hidden');
    this.emptyEl.classList.add('hidden');
    this.errorEl.classList.remove('hidden');
  },

  showEmpty() {
    this.listEl.innerHTML = '';
    this.loadingEl.classList.add('hidden');
    this.emptyEl.classList.remove('hidden');
    this.errorEl.classList.add('hidden');
  },

  render(docs, onSelect) {
    this.loadingEl.classList.add('hidden');
    this.errorEl.classList.add('hidden');

    if (docs.length === 0) {
      this.emptyEl.classList.remove('hidden');
      this.listEl.innerHTML = '';
      return;
    }

    this.emptyEl.classList.add('hidden');

    this.listEl.innerHTML = docs.map(doc => `
      <li data-slug="${doc.slug}">
        <strong>${doc.title}</strong>
        ${doc.summary ? `<br><small>${doc.summary}</small>` : ''}
      </li>
    `).join('');

    this.listEl.querySelectorAll('li').forEach(li => {
      li.addEventListener('click', () => {
        const slug = li.dataset.slug;
        const doc = docs.find(d => d.slug === slug);
        if (doc) onSelect(doc);
      });
    });
  },
};
