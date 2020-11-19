const users = [...document.getElementById("select-users").children].map(u => {
    return {
        name: u.dataset.name,
        email: u.dataset.email,
        id: u.value
	}
})

const admins = [...document.getElementById("select-admins").children].map(u => {
    return {
        name: u.dataset.name,
        email: u.dataset.email,
        id: u.value
    }
})

const teams = [...document.getElementById("select-teams").children].map(u => {
    return {
        name: u.dataset.name,
        id: u.value
    }
})

var REGEX_EMAIL = '([a-z0-9!#$%&\'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&\'*+/=?^_`{|}~-]+)*@' +
    '(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)';

$('#select-users').selectize({
    persist: false,
    maxItems: null,
    valueField: 'id',
    labelField: 'name',
    searchField: ['name', 'email'],
    plugins: ['remove_button'],
    delimiter: ',',
    options: users,
    render: {
        item: function (item, escape) {
            return '<div class="item">' +
                (item.name ? '<span class="name">' + escape(item.name) + '</span>' : '') +
                (item.email ? '<span class="email"> &lt;' + escape(item.email) + '&gt;</span>' : '') +
                '</div>';
        },
        option: function (item, escape) {
            var label = item.name || item.email;
            var caption = item.name ? item.email : null;
            return '<div>' +
                '<span class="label">' + escape(label) + '</span>' +
                (caption ? '<span class="caption"> &lt;' + escape(caption) + '&gt;</span>' : '') +
                '</div>';
        }
    },
    createFilter: function (input) {
        var match, regex;

        // email@address.com
        regex = new RegExp('^' + REGEX_EMAIL + '$', 'i');
        match = input.match(regex);
        if (match) return !this.options.hasOwnProperty(match[0]);

        // name <email@address.com>
        regex = new RegExp('^([^<]*)\<' + REGEX_EMAIL + '\>$', 'i');
        match = input.match(regex);
        if (match) return !this.options.hasOwnProperty(match[2]);

        return false;
    },
    create: function (input) {
        if ((new RegExp('^' + REGEX_EMAIL + '$', 'i')).test(input)) {
            return { email: input };
        }
        var match = input.match(new RegExp('^([^<]*)\<' + REGEX_EMAIL + '\>$', 'i'));
        if (match) {
            return {
                email: match[2],
                name: $.trim(match[1])
            };
        }
        alert('Invalid email address.');
        return false;
    }
});

$('#select-admins').selectize({
    persist: false,
    maxItems: null,
    valueField: 'id',
    labelField: 'name',
    searchField: ['name', 'email'],
    plugins: ['remove_button'],
    delimiter: ',',
    options: admins,
    render: {
        item: function (item, escape) {
            return '<div>' +
                (item.name ? '<span class="name">' + escape(item.name) + '</span>' : '') +
                (item.email ? '<span class="email">' + escape(item.email) + '</span>' : '') +
                '</div>';
        },
        option: function (item, escape) {
            var label = item.name || item.email;
            var caption = item.name ? item.email : null;
            return '<div>' +
                '<span class="label">' + escape(label) + '</span>' +
                (caption ? '<span class="caption">' + escape(caption) + '</span>' : '') +
                '</div>';
        }
    },
    createFilter: function (input) {
        var match, regex;

        // email@address.com
        regex = new RegExp('^' + REGEX_EMAIL + '$', 'i');
        match = input.match(regex);
        if (match) return !this.options.hasOwnProperty(match[0]);

        // name <email@address.com>
        regex = new RegExp('^([^<]*)\<' + REGEX_EMAIL + '\>$', 'i');
        match = input.match(regex);
        if (match) return !this.options.hasOwnProperty(match[2]);

        return false;
    },
    create: function (input) {
        if ((new RegExp('^' + REGEX_EMAIL + '$', 'i')).test(input)) {
            return { email: input };
        }
        var match = input.match(new RegExp('^([^<]*)\<' + REGEX_EMAIL + '\>$', 'i'));
        if (match) {
            return {
                email: match[2],
                name: $.trim(match[1])
            };
        }
        alert('Invalid email address.');
        return false;
    }
});

$('#select-teams').selectize({
    persist: false,
    maxItems: null,
    valueField: 'id',
    labelField: 'name',
    searchField: ['name'],
    plugins: ['remove_button'],
    delimiter: ',',
    options: teams,
    render: {
        item: function (item, escape) {
            return '<div>' +
                (item.name ? '<span class="name">' + escape(item.name) + '</span>' : '') +
                (item.email ? '<span class="email">' + escape(item.email) + '</span>' : '') +
                '</div>';
        },
        option: function (item, escape) {
            var label = item.name || item.email;
            var caption = item.name ? item.email : null;
            return '<div>' +
                '<span class="label">' + escape(label) + '</span>' +
                (caption ? '<span class="caption">' + escape(caption) + '</span>' : '') +
                '</div>';
        }
    },
    createFilter: function (input) {
        var match, regex;

        // email@address.com
        regex = new RegExp('^' + REGEX_EMAIL + '$', 'i');
        match = input.match(regex);
        if (match) return !this.options.hasOwnProperty(match[0]);

        // name <email@address.com>
        regex = new RegExp('^([^<]*)\<' + REGEX_EMAIL + '\>$', 'i');
        match = input.match(regex);
        if (match) return !this.options.hasOwnProperty(match[2]);

        return false;
    },
    create: function (input) {
        if ((new RegExp('^' + REGEX_EMAIL + '$', 'i')).test(input)) {
            return { email: input };
        }
        var match = input.match(new RegExp('^([^<]*)\<' + REGEX_EMAIL + '\>$', 'i'));
        if (match) {
            return {
                email: match[2],
                name: $.trim(match[1])
            };
        }
        alert('Invalid email address.');
        return false;
    }
});