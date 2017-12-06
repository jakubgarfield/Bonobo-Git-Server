;(function ($) {
    'use strict';

    /* TODO: when jQuery will be updated - replace subscription by:
     *             $('.group').on('click', '.toggle', function () {
     */
    $('.group .toggle').click('click', function () {
        var self = $(this),
            repositories = self.closest('.pure-u-1').next('.pure-u-1'),
            group = self.data('group');

        repositories.toggle();

        self.toggleClass('on');

        if (window.localStorage) {
            if (self.hasClass('on')){
                window.localStorage.setItem(group, "false");
            } else{
                window.localStorage.removeItem(group);
            }
        }
    });

    if (window.localStorage) {
        $('.group .toggle').each(function () {
            var self = $(this),
                group = self.data('group');

            if (window.localStorage.getItem(group) === "false") {
                self.click();
            }
        });
    }
})(jQuery);