// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    $('#exampleModal').on('shown.bs.modal', function () {
        $('#myInput').trigger('focus')
    })

    $('#exampleModal #buttonSubmit').on("click", function () {
        
        $.get("/home/encode", { text: $("#myString").val() }).done(function (result) { console.log(result.url); }).always(function () {
            $('#exampleModal').modal('hide');
        });


    });
});


