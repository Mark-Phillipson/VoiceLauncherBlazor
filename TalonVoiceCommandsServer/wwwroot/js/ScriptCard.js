// Usage: ScriptCard.render({ title: 'JavaScript', lines: [ 'let x = 1;', ... ] }, container)
// ScriptCard.js: Reusable JS/HTML for displaying code blocks with copy features
// Usage: ScriptCard.render({ title: 'JavaScript', lines: [ 'let x = 1;', ... ] }, container)

const ScriptCard = {
  render({ title, lines }, container) {
    // Create card
    const card = document.createElement('div');
    card.className = 'card mb-3';
    card.style.maxWidth = '600px';
    card.innerHTML = `
      <div class="card-header d-flex justify-content-between align-items-center">
        <span class="fw-bold">${title}</span>
        <button class="btn btn-sm btn-outline-secondary" id="copy-all" title="Copy all lines" aria-label="Copy all lines">
          <i class="fa fa-copy"></i> Copy
        </button>
      </div>
      <div class="card-body p-0">
        <pre class="m-0" style="background:#f6f8fa; border-radius:0;">
          <code id="code-block" style="display:block;">
            ${lines.map((line, i) => `
              <div class="d-flex align-items-center py-1 px-2" style="border-bottom:1px solid #eee;">
                <button class="btn btn-xs btn-light me-2" style="width:2em;" title="Copy line ${i+1}" aria-label="Copy line ${i+1}" data-line="${i}">${i+1}</button>
                <span style="font-family:monospace;">${line}</span>
              </div>
            `).join('')}
          </code>
        </pre>
      </div>
    `;
    container.innerHTML = '';
    container.appendChild(card);

    // Copy all lines
    card.querySelector('#copy-all').onclick = () => {
      navigator.clipboard.writeText(lines.join('\n'));
    };
    // Copy individual lines
    card.querySelectorAll('button[data-line]').forEach(btn => {
      btn.onclick = () => {
        const idx = parseInt(btn.getAttribute('data-line'));
        navigator.clipboard.writeText(lines[idx]);
      };
    });
  }
};

// Example usage:
// ScriptCard.render({ title: 'JavaScript', lines: [ 'let x = 1;', 'console.log(x);', ... ] }, document.getElementById('card-container'));
