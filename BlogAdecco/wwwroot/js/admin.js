/// Copyright (c) 2021, Mapache Digital
/// Version: 1.5
/// Author: Samuel Kobelkowsky
/// Email: samuel@mapachedigital.com

// Initial stuff
jQuery(function () {
    // Fix missing calendar chooser in Safari
    document.querySelectorAll('input.calendar').forEach(function (element, _index) {
        if (element.type !== 'date') {
            element.datepicker({ dateFormat: 'yy-mm-dd' });
        }
    });

    // Recalculate the margin for the header and footer according to their respective size
    //jQuery(window).resize(footerResized);
    //footerResized();
});

// Generate a random UUID
function createUuid() {
    var dt = new Date().getTime();
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (dt + Math.random() * 16) % 16 | 0;
        dt = Math.floor(dt / 16);
        return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
    return uuid;
}

// Redirect to a new URL where the parameter 'pageSize' is set from the value of a <select>.
// Usage: <form method="get" action="url"><select onchange="changePageSize(this)"><option>10</option>...</select></form>
function changePageSize(select) {
    const uri = new URL(select.form.action);
    const pageSize = select.value;

    uri.searchParams.set('pageSize', pageSize);
    window.location.replace(uri.href);
    return false;
}

// Show a loader and hide the submit button of a form.
// Intended to with jQuery validator as follows:
// jQuery.validator.setDefaults({ submitHandler: showLoaderHideSubmit } });
function showLoaderHideSubmit(form) {
    if (jQuery(form).find('.form-loading').length > 0) {
        jQuery('input[type="submit"]').prop('disabled', true);
        jQuery('button[type="submit"]').prop('disabled', true);
        jQuery(form).find('.form-loading').removeClass('d-none');
        jQuery(form).find('.form-loading').show();
    }

    form.submit();
}