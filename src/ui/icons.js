const iconPaths={
home:'<path d="m3 10 9-7 9 7v10a1 1 0 0 1-1 1h-5v-7H9v7H4a1 1 0 0 1-1-1z"/>',
water:'<path d="M12 3s-7 8-7 12a7 7 0 0 0 14 0c0-4-7-12-7-12Z"/><path d="M8 15a4 4 0 0 0 4 4"/>',
weight:'<rect x="4" y="3" width="16" height="18" rx="3"/><path d="M8 9a4 4 0 0 1 8 0M12 9l2-3"/>',
habits:'<rect x="4" y="4" width="4" height="4" rx="1"/><path d="M12 6h8M12 12h8M12 18h8m-8-6 1.5 1.5L8 10m-4 8 1.5 1.5L8 16"/>',
calendar:'<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M7 3v4m10-4v4M3 11h18m-14 4h2m3 0h2m3 0h1m-11 3h2m3 0h2"/>',
settings:'<path d="m10 3-1 3-3 1-2-1-2 4 2 2v3l-2 2 2 4 3-1 3 1 1 3h4l1-3 3-1 3 1 2-4-2-2v-3l2-2-2-4-3 1-3-1-1-3z" transform="translate(0 -1) scale(.95)"/><circle cx="12" cy="12" r="3"/>',
tooth:'<path d="M12 5c-3-3-8-2-8 3 0 3 1 4 2 7 1 4 1 6 3 6s1-7 3-7 1 7 3 7 2-2 3-6c1-3 2-4 2-7 0-5-5-6-8-3Z"/>',
walk:'<circle cx="14" cy="4" r="2"/><path d="m10 21 2-7-3-3 2-4 4 3 4 1M5 11l4-2m3 5 4 3 1 4"/>',
moon:'<path d="M20 15A8 8 0 0 1 9 4a8 8 0 1 0 11 11Z"/>',
heart:'<path d="M20 5a5 5 0 0 0-8 1 5 5 0 0 0-8-1c-4 4 1 9 8 15 7-6 12-11 8-15Z"/>',
routine:'<rect x="5" y="4" width="14" height="17" rx="2"/><path d="M9 3h6v4H9zM9 12h6m-6 4h6"/>',
check:'<path d="m5 12 4 4L19 6"/>'};
function uiIcon(name){return `<svg class="ui-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${iconPaths[name]||iconPaths.routine}</svg>`}
function routineIcon(key){return uiIcon({'✦':'routine','◈':'tooth','↗':'walk','☾':'moon','◉':'water','♡':'heart'}[key]||'routine')}

iconPaths.meal='<path d="M5 3v6m3-6v6M2 3v6c0 2 6 2 6 0M5 11v10m13-18c-3 2-4 7-1 8h3M20 3v18"/>';iconPaths.measure='<rect x="3" y="7" width="18" height="10" rx="2"/><path d="M7 7v4m4-4v6m4-6v4m4-4v6"/>';

iconPaths.target='<circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="5"/><circle cx="12" cy="12" r="1"/>';iconPaths.alarm='<circle cx="12" cy="13" r="8"/><path d="M12 9v5l3 2M3 5l3-3m12 0 3 3M6 20l-2 2m14-2 2 2"/>';

iconPaths.summary='<path d="M4 21V10m6 11V4m6 17v-7m5 7H2"/>';

