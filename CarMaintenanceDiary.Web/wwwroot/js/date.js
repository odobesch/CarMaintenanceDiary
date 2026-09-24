window.initFlatpickr = (selector, locale = 'cs') => {
    flatpickr(selector, {
        dateFormat: "d.m.Y",
        locale: locale,
        allowInput: true,        
        maxDate: "today"
    });
};