﻿<%@  Page Title="Chat" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
    <div class="container">
        <input type="text" id="message" />
        <input type="button" id="sendmessage" value="Send" />
        <input type="hidden" id="displayname" />
        <ul id="discussion"></ul>
    </div>

        <!--Script references. -->
    <!--Reference the SignalR library. -->
    <script src="<%=Url.Content("~/Scripts/jquery.signalR-2.2.0.js")%>"></script>
    <!--Reference the autogenerated SignalR hub script. script src="/signalr/hubs" /script  -->
    <script src="<%=ViewBag.SignalRHub%>/hubs"></script>
    <!--Add script to update the page and send messages.-->
    <script type="text/javascript">
      $(function () {
            //Set the hubs URL for the connection
            $.connection.hub.url = "<%=ViewBag.SignalRHub%>";

            // Declare a proxy to reference the hub.
            var chat = $.connection.myHub;

            // Create a function that the hub can call to broadcast messages.
            chat.client.addMessage = function (name, message) {
                // Html encode display name and message.
                var encodedName = $('<div />').text(name).html();
                var encodedMsg = $('<div />').text(message).html();
                // Add the message to the page.
                $('#discussion').append('<li><strong>' + encodedName
                    + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');
            };
            // Get the user name and store it to prepend to messages.
            $('#displayname').val(prompt('Enter your name:', ''));
            // Set initial focus to message input box.
            $('#message').focus();
            // Start the connection.
            $.connection.hub.start().done(function () {
                $('#sendmessage').click(function () {
                    // Call the Send method on the hub.
                    chat.server.send($('#displayname').val(), $('#message').val());
                    // Clear text box and reset focus for next comment.
                    $('#message').val('').focus();
                });
            });
        });
    </script>


</asp:Content>