// Kana convert (data-kana-convert の変換を、欄を離れた時と送る時にかける)
$(function () {
    const wide = ["RomanToWide", "NumericToWide", "AsciiToWide", "SpaceToWide", "HankanaToKatakana"];

    const zip = (source, target) => Array.from(source, (c, i) => [c, target[i]]);

    const between = (code, start, end) => (code >= start) && (code <= end);

    const asciiToWide = new Map([
        ...zip("!\"#$%&'()*+,-./", "！”＃＄％＆’（）＊＋，－．／"),
        ...zip(":;<=>?@", "：；＜＝＞？＠"),
        ...zip("[\\]^_`", "［￥］＾＿｀"),
        ...zip("{|}~", "｛｜｝～"),
        ["¥", "￥"]
    ]);

    const voiced = new Map(zip("ｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾊﾋﾌﾍﾎｳﾜｦ", "ガギグゲゴザジズゼゾダヂヅデドバビブベボヴヷヺ"));

    const semiVoiced = new Map(zip("ﾊﾋﾌﾍﾎ", "パピプペポ"));

    const hankana = "。「」、・ヲァィゥェォャュョッーアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワン゛゜";

    function parse(option) {
        const options = new Set();
        for (const name of option.split(",")) {
            const value = name.trim();
            if (value === "Wide") {
                wide.forEach((x) => options.add(x));
            } else {
                options.add(value);
            }
        }
        return options;
    }

    function convert(value, options) {
        let result = "";
        for (let i = 0; i < value.length; i++) {
            const c = value[i];
            const code = c.charCodeAt(0);
            if (options.has("SpaceToWide") && (c === " ")) {
                result += "　";
            } else if (options.has("NumericToNarrow") && between(code, 0xff10, 0xff19)) {
                result += String.fromCharCode(code - 0xfee0);
            } else if (options.has("NumericToWide") && between(code, 0x30, 0x39)) {
                result += String.fromCharCode(code + 0xfee0);
            } else if (options.has("RomanToNarrow") && (between(code, 0xff21, 0xff3a) || between(code, 0xff41, 0xff5a))) {
                result += String.fromCharCode(code - 0xfee0);
            } else if (options.has("RomanToWide") && (between(code, 0x41, 0x5a) || between(code, 0x61, 0x7a))) {
                result += String.fromCharCode(code + 0xfee0);
            } else if (options.has("AsciiToWide") && asciiToWide.has(c)) {
                result += asciiToWide.get(c);
            } else if (options.has("HiraganaToKatakana") && (between(code, 0x3041, 0x3096) || (code === 0x309d) || (code === 0x309e))) {
                result += String.fromCharCode(code + 0x60);
            } else if (options.has("HankanaToKatakana") && between(code, 0xff61, 0xff9f)) {
                const next = value[i + 1];
                if ((next === "ﾞ") && voiced.has(c)) {
                    result += voiced.get(c);
                    i++;
                } else if ((next === "ﾟ") && semiVoiced.has(c)) {
                    result += semiVoiced.get(c);
                    i++;
                } else {
                    result += hankana[code - 0xff61];
                }
            } else {
                result += c;
            }
        }
        return result;
    }

    function apply(element) {
        const value = convert(element.value, parse(element.dataset.kanaConvert));
        if (value !== element.value) {
            element.value = value;
        }
    }

    $(document).on("change", "input[data-kana-convert]", function () {
        apply(this);
    });

    document.addEventListener("submit", function (event) {
        $(event.target).find("input[data-kana-convert]").each(function () {
            apply(this);
        });
    }, true);
});
