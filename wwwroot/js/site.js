// Please see documentation at https://docs.microsoft.com/;aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Quick and simple export target #table_id into a csv
function download_table_as_csv(table_id, separator = ';') {
    // Select rows from table_id
    var rows = document.querySelectorAll('table#' + table_id + ' tr');
    // Construct csv
    var csv = [];
    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll('td, th');
        for (var j = 0; j < cols.length; j++) {
            // Clean innertext to remove multiple spaces and jumpline (break csv)
            var data = cols[j].innerText.replace(/(\r\n|\n|\r)/gm, '').replace(/(\s\s)/gm, ' ')
            // Escape double-quote with double-double-quote (see https://stackoverflow.com/questions/17808511/properly-escape-a-double-quote-in-csv)
            data = data.replace(/"/g, '""');
            // Push escaped string
            row.push('"' + data + '"');
        }
        csv.push(row.join(separator));
    }
    var csv_string = csv.join('\n');
    // Download it
    var filename = 'export_' + table_id + '_' + new Date().toLocaleDateString() + '.csv';
    var link = document.createElement('a');
    link.style.display = 'none';
    link.setAttribute('target', '_blank');
    link.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv_string));
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

let debounceTimeoutId;
function debounce(func, delay) {
    return function(...args) {
        clearTimeout(debounceTimeoutId);
        debounceTimeoutId = setTimeout(() => {
            console.log('debounce trigger');
            func.apply(this, args);
        }, delay);
    };
}

const statlinks = document.querySelectorAll('.stats .filter');
let filter = null;
for (let i = 0; i < statlinks.length; i++) {
        statlinks[i].addEventListener('click', (e) => {
        statlinks.forEach(l => {l.classList.remove('selected')});
        let insightName = e.target.dataset.appname;
        if (filter === insightName) {
            insightName = ''
            e.target.classList.remove('selected');
        } else {
            e.target.classList.add('selected');
        }
        filter = insightName;
        const rows = document.querySelectorAll('table tr');
        for (let r = 0; r < rows.length; r++) {
            if (rows[r].innerText.indexOf(filter) === -1) {
                rows[r].classList.add('hidefilter');
            } else {
                rows[r].classList.remove('hidefilter');
            }
        }
    });
}

const search = document.querySelector('input[name="search"]');

function searchTable() {
    clearTimeout(debounceTimeoutId);
    console.log('search', search.value);
    const query = search.value
    const rows = document.querySelectorAll('table tr');
    for (let r = 0; r < rows.length; r++) {
        if (rows[r].innerText.indexOf(query) === -1) {
            rows[r].classList.add('hidesearch');
        } else {
            rows[r].classList.remove('hidesearch');
        }
    }
    const cntVisible = document.querySelectorAll('table tr:not(.hidesearch, .hidefilter)').length;
    document.getElementById('totalFound').innerText = cntVisible;
}

if (search) {
    search.addEventListener('keyup', debounce(searchTable, 300));
    search.addEventListener('search', searchTable);
}
