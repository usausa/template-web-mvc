// Dirty behavior (未保存離脱警告)
$(function () {
    $(document).on("change", "form.dirty-behavior :input", function () {
        $(this).closest(".dirty-behavior").data("dirty", true);
    });

    $(window).on("beforeunload", function () {
        let warn = false;

        $(".dirty-behavior").each(function () {
            if ($(this).data("dirty")) {
                warn = true;
                return false;
            }
            return true;
        });

        return warn ? true : undefined;
    });

    $(document).on("submit", "form", function () {
        if (($(this).attr("method") || "get").toLowerCase() !== "get") {
            $(window).off("beforeunload");
        }
    });
});

// Confirm behavior (送信前の確認。メッセージは data-confirm)
$(function () {
    $(document).on("submit", "form[data-confirm]", function () {
        return confirm($(this).data("confirm"));
    });
});
