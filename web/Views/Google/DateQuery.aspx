<%@ Page Title="Date search" Language="C#" Inherits="System.Web.Mvc.ViewPage<AskForADate>" MasterPageFile="~/Models/App.master" %>


<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">

 <script type="text/javascript" src="/Scripts/jquery-2.1.1.min.js"></script>

  <script type="text/javascript" src="/Scripts/jquery.timepicker.js"></script>
  <script type="text/javascript" src="/Scripts/jquery.mousewheel.js"></script>
  <link rel="stylesheet" type="text/css" href="/Theme/jquery.timepicker.css" />
  <script type="text/javascript" src="/Scripts/jquery-ui.js"></script>
  <link rel="stylesheet" href="/Theme/jquery-ui.css">
  <style>

 .ui-icon .ui-icon-circle-triangle-e {
  background-image: url(/images/ui-bg_flechg.png);
    }
.ui-datepicker-next .ui-corner-all {
  background-image: url(/images/ui-bg_flechd.png);
}
 

  </style>
   <script>
$.widget( "ui.timespinner", $.ui.spinner, {
options: {
// seconds
step: 60 * 1000,
// hours
page: 60
},
_parse: function( value ) {
if ( typeof value === "string" ) {
// already a timestamp
if ( Number( value ) == value ) {
return Number( value );
}
return +Globalize.parseDate( value );
}
return value;
},
_format: function( value ) {
return Globalize.format( new Date(value), "t" );
}
});
   </script>
</asp:Content>

  <asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">


<% using ( Html.BeginForm("DateQuery","Google") ) { %>

  <p>Période de recherche:</p>
  <p>
  <%= Html.LabelFor(model=>model.MinDate) %>:<br>
 Le <input type="text" id="MinDate">
à
  <input type="text" id="MinTime" class="time"/>
  <%= Html.ValidationMessageFor(model=>model.MinDate) %>
  </p>

  <p>
<%= Html.LabelFor(model=>model.MaxDate) %>:<br>
  Le  <input type="text" id="MaxDate">
 à
  <input type="text" id="MaxTime" class="time"/>
  <%= Html.ValidationMessageFor(model=>model.MaxDate) %>
  </p>

  <p>
  Durée minimale : <input type="text" id="Duration" class="time"/>
  </p>

  <p>
  <%= Html.LabelFor(model=>model.UserName) %>:<br>
  <%= Html.TextBoxFor(model=>model.UserName) %><br>
  <%= Html.ValidationMessageFor(model=>model.UserName) %>
  </p>

  <script>
                $(function() {
              
                    $('#MinTime').timepicker({ 'scrollDefault': 'now' });
                    $('#MaxTime').timepicker({ 'scrollDefault': 'now' });
                    $( "#MinDate" ).datepicker();
                    $( "#MaxDate" ).datepicker();
                    $( "#Duration" ).timespinner();

                });
  </script>


<input type="submit">
<% } %>
<pre><%= Html.Encode(ViewData["json"]) %></pre>

</asp:Content>
