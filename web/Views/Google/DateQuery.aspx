<%@ Page Title="Date search" Language="C#" Inherits="System.Web.Mvc.ViewPage<AskForADate>" MasterPageFile="~/Models/App.master" %>


<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">

 <script type="text/javascript" src="/Scripts/jquery-2.1.3.min.js"></script>
  <script type="text/javascript" src="/Scripts/jquery.mousewheel.js"></script>
  <script type="text/javascript" src="/Scripts/jquery-ui.js"></script>
  <script type="text/javascript" src="/Scripts/jquery.timepicker.js"></script>
  <link rel="stylesheet" type="text/css" href="/Theme/jquery.timepicker.css" />
 <script type="text/javascript" src="/Scripts/globalize/globalize.js"></script>
 <script type="text/javascript" src="/Scripts/globalize/cultures/globalize.culture.fr.js"></script>
 <script type="text/javascript" src="/Scripts/datepicker-fr.js"></script>
 <script type="text/javascript" src="/Scripts/datepicker-en-GB.js"></script>
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
 Le <%= Html.TextBoxFor(model=>model.MinDate) %>
<%= Html.ValidationMessageFor(model=>model.MinDate) %>
à
  <%= Html.TextBoxFor(model=>model.MinTime) %>
  <%= Html.ValidationMessageFor(model=>model.MinTime) %>
  </p>

  <p>
<%= Html.LabelFor(model=>model.MaxDate) %>:<br>
  Le  <%= Html.TextBoxFor(model=>model.MaxDate) %>
  <%= Html.ValidationMessageFor(model=>model.MaxDate) %>
 à
  <%= Html.TextBoxFor(model=>model.MaxTime) %>
  <%= Html.ValidationMessageFor(model=>model.MaxTime) %>
  </p>

  <p>
  Durée minimale : <%= Html.TextBoxFor(model=>model.Duration) %>
   <%= Html.ValidationMessageFor(model=>model.Duration) %>
  </p>

  <p>
  <%= Html.LabelFor(model=>model.UserName) %>:<br>
  <%= Html.TextBoxFor(model=>model.UserName) %><br>
  <%= Html.ValidationMessageFor(model=>model.UserName) %>
  </p>

  <script>
    $(function() {
       Globalize.culture("fr");
        $( '#MinTime' ).timepicker({ 'scrollDefault': 'now', 'timeFormat': 'H:i'  });
        $( '#MaxTime' ).timepicker({ 'scrollDefault': 'now', 'timeFormat': 'H:i' });
        $( "#MinDate" ).datepicker(	$.datepicker.regional["fr"] );
        $( "#MaxDate" ).datepicker( $.datepicker.regional["fr"] );
        $( "#Duration" ).timespinner();
    });
  </script>

<input type="submit">
<% } %>
<pre><%= Html.Encode(ViewData["json"]) %></pre>

</asp:Content>
