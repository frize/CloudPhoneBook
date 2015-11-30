<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CloudDictionary.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dictionary</title>
    <script src="//code.jquery.com/jquery-1.11.3.min.js"></script>
    <script src="//code.jquery.com/jquery-migrate-1.2.1.min.js"></script>

    <script>
        $(document).ready(function () {

            function search(str) {
                $.ajax({
                    url: "Dictionary.asmx/GetAllContact",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: "{}",
                    success: showSearchResult
                });
            }

            function showSearchResult(data) {
                /*
                public string name;
                public string gender;
                public string age;
                public string number;
                public string city;
                */
                var html = "<ul>";
                for (var i = 0; i < data.d.length ; i++) {
                    var contact = data.d[i];
                    html += "<li>" + contact.name + "</li>" +
                            "<ul>" +
                                "<li>Gender: " + contact.gender + "</li>" +
                                "<li>Age: " + contact.age + "</li>" +
                                "<li>Number: " + contact.number + "</li>" +
                                "<li>City: " + contact.city + "</li>" +
                            "</ul>" +
                            "</li>";
                }
                html += "</ul>";
                $("#search_result").html(html);
            }

            /*
            var timeoutSearchTyping = null;
            $('#search_text').keyup(function () {
                clearTimeout(timeoutSearchTyping);
                var $target = $(this);
                timeoutSearchTyping = setTimeout(function () { search($target.val()); }, 1000);
            });
            */
            $('#search_text').keypress(function (e) {
                if (e.which == 13) {
                    var $target = $(this);
                    search($target.val());
                }
            });
            $("#search_button").click(function () {
                search($("#search_text").val());
            });

        });
    </script>
</head>
<body>
    <div style="margin: auto; text-align: left; vertical-align: middle; line-height: normal;">
        <span>Search: </span>
        <input type="text" name="search_text" id="search_text" />
        <input type="button" id="search_button" value="Search" />
    </div>
    <div id="space">&nbsp;&nbsp;
        <br />
        <br />
        <br />
    </div>
    <div id="search_result">
    </div>
</body>
</html>
