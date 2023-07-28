$(function () {
    app.initialize();

    // 啟用 Knockout
    ko.validation.init({ grouping: { observable: false } });
    ko.applyBindings(app, document.body);
});
