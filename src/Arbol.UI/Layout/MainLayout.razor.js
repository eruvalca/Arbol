export function initializeNavigation(blazor) {
    // The navigation panel is a CSS disclosure, so enhanced navigation must
    // collapse it before Blazor patches the next page into the document.
    blazor.addEventListener('enhancednavigationstart', () => {
        const toggle = document.getElementById('site-menu-toggle');
        if (toggle instanceof HTMLInputElement) {
            toggle.checked = false;
        }
    });
}
