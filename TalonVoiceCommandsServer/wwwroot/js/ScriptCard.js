// Usage: ScriptCard.render({ title: 'JavaScript', lines: [ 'let x = 1;', ... ] }, container)
// ScriptCard.js: Reusable JS/HTML for displaying code blocks with copy features
// Usage: ScriptCard.render({ title: 'JavaScript', lines: [ 'let x = 1;', ... ] }, container)

// Prevent multiple declarations
if (typeof window.ScriptCard === 'undefined') {
  window.ScriptCard = {
    render({ title, lines }, container) {
      // Filter out empty lines for determining actual content
      const nonEmptyLines = lines.filter(line => line.trim() !== '');
      
      // Create GitHub-style card
      const card = document.createElement('div');
  card.className = 'card mb-3 border';
  // Use CSS variables so the card respects Bootstrap light/dark themes
  card.style.cssText = 'border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06);';
      
      // Check if we have exactly one line of content
      if (nonEmptyLines.length === 1) {
        // Single line layout - everything on one row
        // Use Bootstrap CSS variables so this adapts to light/dark theme
        card.innerHTML = `
          <div class="d-flex align-items-center py-1 px-2" style="background-color: var(--bs-body-bg); border: 1px solid var(--bs-border-color-translucent); border-radius: 6px;">
            <span class="fw-bold text-body-secondary me-2" style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui; font-size: 11px;">${this.escapeHtml(title)}</span>
            <code class="flex-grow-1 me-2" style="background: var(--bs-body-bg); color: var(--bs-body-color); padding: 2px 6px; border: 1px solid var(--bs-border-color-translucent); border-radius: 3px; font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace; font-size: 11px; white-space: nowrap; overflow-x: auto; line-height: 16px;">${this.escapeHtml(nonEmptyLines[0])}</code>
            <button class="btn btn-sm flex-shrink-0" id="copy-single-${Date.now()}" title="Copy script" aria-label="Copy script" style="background: transparent; border: 1px solid var(--bs-border-color-translucent); border-radius: 4px; padding: 2px 6px; font-size: 11px; font-weight: 500; color: var(--bs-body-color);">
              <svg aria-hidden="true" height="14" viewBox="0 0 16 16" version="1.1" width="14" style="fill: currentColor;">
                <path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path>
                <path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path>
              </svg>
              Copy
            </button>
          </div>
        `;
      } else {
        // Multi-line layout - simple text display without line numbers
        
        card.innerHTML = `
          <div class="card-header d-flex justify-content-between align-items-center py-1 px-2" style="background-color: var(--bs-body-bg); border-bottom: 1px solid var(--bs-border-color-translucent);">
            <span class="fw-bold text-body" style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui; font-size: 12px;">${this.escapeHtml(title)}</span>
            <button class="btn btn-sm" id="copy-all-${Date.now()}" title="Copy all lines" aria-label="Copy all lines" style="background: transparent; border: 1px solid var(--bs-border-color-translucent); border-radius: 4px; padding: 2px 6px; font-size: 11px; font-weight: 500; color: var(--bs-body-color);">
              <svg aria-hidden="true" height="14" viewBox="0 0 16 16" version="1.1" width="14" style="fill: currentColor;">
                <path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path>
                <path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path>
              </svg>
              Copy
            </button>
          </div>
          <div class="card-body p-2" style="background-color: var(--bs-body-bg);">
            <pre style="background: var(--bs-body-bg); border: 1px solid var(--bs-border-color-translucent); border-radius: 4px; padding: 8px; margin: 0; font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace; font-size: 11px; line-height: 1.3; white-space: pre-wrap; overflow-x: auto; color: var(--bs-body-color);">${this.escapeHtml(lines.filter(line => line.trim() !== '').join('\n'))}</pre>
          </div>
        `;
      }
      
      container.innerHTML = '';
      container.appendChild(card);

      // Handle copy functionality based on layout type
      if (nonEmptyLines.length === 1) {
        // Single line copy functionality
        const copySingleBtn = card.querySelector(`[id^="copy-single-"]`);
        if (copySingleBtn) {
          copySingleBtn.onclick = () => {
            navigator.clipboard.writeText(nonEmptyLines[0]);
                  this.showCopyFeedback(copySingleBtn, 'Copied!');
          };
        }
      } else {
        // Multi-line copy functionality - only "copy all" button
        const copyAllBtn = card.querySelector(`[id^="copy-all-"]`);
        if (copyAllBtn) {
          copyAllBtn.onclick = () => {
            navigator.clipboard.writeText(lines.filter(line => line.trim() !== '').join('\n'));
            this.showCopyFeedback(copyAllBtn, 'Copied!');
          };
        }
      }
    },

    // Helper function to show copy feedback
    showCopyFeedback(button, message) {
      const originalText = button.innerHTML;
      button.innerHTML = message;
      // Use Bootstrap success variable so the feedback color works in both themes
      button.style.color = 'var(--bs-success)';
      setTimeout(() => {
        button.innerHTML = originalText;
        button.style.color = '';
      }, 1000);
    },

    // Helper function to escape HTML
    escapeHtml(text) {
      if (!text) return '';
      const div = document.createElement('div');
      div.textContent = text;
      return div.innerHTML;
    }
  }; // End of window.ScriptCard definition
} // End of if statement

// Example usage:
// ScriptCard.render({ title: 'JavaScript', lines: [ 'let x = 1;', 'console.log(x);', ... ] }, document.getElementById('card-container'));
