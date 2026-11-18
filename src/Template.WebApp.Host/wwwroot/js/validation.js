// Shift_JIS byte length
(function ($) {
    function ms932Length(value) {
        let length = 0;
        for (let i = 0; i < value.length; i++) {
            const c = value.charCodeAt(i);
            length += (c <= 0x7f) || ((c >= 0xff61) && (c <= 0xff9f)) ? 1 : 2;
        }
        return length;
    }

    $.validator.addMethod("ms932length", function (value, element, params) {
        if (this.optional(element)) {
            return true;
        }

        const length = ms932Length(value);
        return (length >= params.min) && (length <= params.max);
    });

    $.validator.unobtrusive.adapters.add("ms932length", ["min", "max"], function (options) {
        options.rules.ms932length = {
            min: parseInt(options.params.min, 10),
            max: parseInt(options.params.max, 10)
        };
        options.messages.ms932length = options.message;
    });
})(jQuery);

// Character type
(function ($) {
    $.validator.addMethod("charactertype", function (value, element, pattern) {
        if (this.optional(element)) {
            return true;
        }

        const match = new RegExp(pattern).exec(value);
        return (match !== null) && (match.index === 0) && (match[0].length === value.length);
    });

    $.validator.unobtrusive.adapters.addSingleVal("charactertype", "pattern");
})(jQuery);
