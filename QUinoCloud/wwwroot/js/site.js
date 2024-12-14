$(document).ready(function () {
    $("select.togglebox").change(function () {
        $(this).find("option:selected").each(function () {
            var optionValue = $(this).attr("value");
            if (optionValue) {
                $(".togglebox-data>div").not("#" + optionValue).hide();
                $("#" + optionValue).show();
            } else {
                $(".togglebox-data>div").hide();
            }
        });
    }).change();
});