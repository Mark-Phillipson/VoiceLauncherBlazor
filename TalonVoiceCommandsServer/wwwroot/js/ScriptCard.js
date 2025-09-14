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
      card.style.cssText = 'border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1);';
      
      // Check if we have exactly one line of content
      if (nonEmptyLines.length === 1) {
        // Single line layout - everything on one row
        card.innerHTML = `
          <div class="d-flex align-items-center py-2 px-3" style="background-color: #f6f8fa; border: 1px solid #d0d7de; border-radius: 8px;">
            <span class="fw-bold text-dark me-3" style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui; font-size: 12px; color: #656d76;">${this.escapeHtml(title)}</span>
            <code class="flex-grow-1 me-3" style="background: white; color: #24292f; padding: 4px 8px; border: 1px solid #d0d7de; border-radius: 4px; font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace; font-size: 12px; white-space: nowrap; overflow-x: auto;">${this.escapeHtml(nonEmptyLines[0])}</code>
            <button class="btn btn-sm flex-shrink-0" id="copy-single-${Date.now()}" title="Copy script" aria-label="Copy script" style="background: #f6f8fa; border: 1px solid #d0d7de; border-radius: 6px; padding: 4px 8px; font-size: 12px; font-weight: 500;">
              <svg aria-hidden="true" height="16" viewBox="0 0 16 16" version="1.1" width="16" style="fill: #656d76;">
                <path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path>
                <path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path>
              </svg>
              Copy
            </button>
          </div>
        `;
      } else {
        // Multi-line layout - show only actual lines without padding
        
        card.innerHTML = `
          <div class="card-header d-flex justify-content-between align-items-center py-2 px-3" style="background-color: #f6f8fa; border-bottom: 1px solid #d0d7de;">
            <span class="fw-bold text-dark" style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui;">${this.escapeHtml(title)}</span>
            <button class="btn btn-sm" id="copy-all-${Date.now()}" title="Copy all lines" aria-label="Copy all lines" style="background: #f6f8fa; border: 1px solid #d0d7de; border-radius: 6px; padding: 4px 8px; font-size: 12px; font-weight: 500;">
              <svg aria-hidden="true" height="16" viewBox="0 0 16 16" version="1.1" width="16" style="fill: #656d76;">
                <path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path>
                <path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path>
              </svg>
              Copy
            </button>
          </div>
          <div class="card-body p-0" style="background-color: #f6f8fa;">
            <div style="border-radius: 0;">
              <table class="table table-sm mb-0" style="background: transparent; font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;">
                <tbody>
                  ${lines.map((line, i) => `
                    <tr style="border: none;">
                      <td class="pe-2 text-center" style="width: 50px; border: none; background: #f6f8fa; color: #656d76; font-size: 12px; line-height: 20px; padding: 4px 8px; border-right: 1px solid #d0d7de; user-select: none;">
                        <button class="btn btn-link p-0 text-decoration-none line-btn" 
                                style="color: #656d76; font-size: 12px; border: none; background: none; width: 100%; text-align: center; cursor: pointer;" 
                                title="Copy line ${i+1}" 
                                aria-label="Copy line ${i+1}" 
                                data-line="${i}">${i+1}</button>
                      </td>
                      <td style="border: none; background: white; padding: 4px 8px; font-size: 12px; line-height: 20px; white-space: pre-wrap; font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;">
                        <code style="background: none; color: #24292f; padding: 0; border: none;">${this.escapeHtml(line)}</code>
                      </td>
                    </tr>
                  `).join('')}
                </tbody>
              </table>
            </div>
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
        // Multi-line copy functionality
        const copyAllBtn = card.querySelector(`[id^="copy-all-"]`);
        if (copyAllBtn) {
          copyAllBtn.onclick = () => {
            navigator.clipboard.writeText(lines.filter(line => line.trim() !== '').join('\n'));
            this.showCopyFeedback(copyAllBtn, 'Copied!');
          };
        }
        
        // Copy individual lines functionality
        card.querySelectorAll('button[data-line]').forEach(btn => {
          btn.onclick = () => {
            const idx = parseInt(btn.getAttribute('data-line'));
            if (lines[idx] && lines[idx].trim() !== '') {
              navigator.clipboard.writeText(lines[idx]);
              this.showCopyFeedback(btn, 'Copied!');
            }
          };
        });
      }
    },

    // Helper function to show copy feedback
    showCopyFeedback(button, message) {
      const originalText = button.innerHTML;
      button.innerHTML = message;
      button.style.color = '#2da44e';
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
