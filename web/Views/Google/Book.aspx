<%@ Page Title="Booking" Language="C#" Inherits="System.Web.Mvc.ViewPage<BookQuery>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
<link rel='stylesheet' href='/Scripts/fullcalendar/fullcalendar.css' />
 <script type="text/javascript" src="/Scripts/jquery-2.1.3.min.js"></script>
<script src='/Scripts/fullcalendar/lib/moment.min.js'></script>
<script src='/Scripts/fullcalendar/fullcalendar.js'></script>
  <script type="text/javascript" src="/Scripts/jquery.mousewheel.js"></script>
  <script type="text/javascript" src="/Scripts/jquery.timepicker.js"></script>
  <link rel="stylesheet" type="text/css" href="/App_Themes/jquery.timepicker.css" />
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

<% using ( Html.BeginForm("Book","Google") ) { %>
<fieldset><legend>Période de recherche</legend>

<%= Html.LabelFor(model=>model.PreferedDate) %>:
  <%= Html.TextBoxFor(model => model.PreferedDate) %>
  <%= Html.ValidationMessageFor(model=>model.PreferedDate) %>
<br>
<%= Html.LabelFor(model=>model.PreferedHour) %>:
  <%= Html.TextBoxFor(model=>model.PreferedHour) %>
  <%= Html.ValidationMessageFor(model=>model.PreferedHour) %>
  <br>
<%= Html.LabelFor(model=>model.MaxDate) %>:
  <%= Html.TextBoxFor(model=>model.MaxDate) %>
  <%= Html.ValidationMessageFor(model=>model.MaxDate) %>
 <br>
<%= Html.LabelFor(model=>model.MinDuration) %>:
  <%= Html.TextBoxFor(model=>model.MinDuration) %>
   <%= Html.ValidationMessageFor(model=>model.MinDuration) %>
  <br>
  <%= Html.LabelFor(model=>model.Role) %>:
  <%= Html.TextBoxFor(model=>model.Role) %>
  <%= Html.ValidationMessageFor(model=>model.Role) %>
 <br>
  <%= Html.LabelFor(model=>model.Person) %>:
  <%= Html.TextBoxFor(model=>model.Person) %>
  <%= Html.ValidationMessageFor(model=>model.Person) %>
  </fieldset>
  <script>
    $(function() {
    var dpconfig = $.datepicker.regional["fr"];
      $( '#PreferedHour' ).addClass('ui-timepicker-input');
      $( '#PreferedHour' ).timepicker({ 'scrollDefault': 'now', 'timeFormat': 'H:i'  });
      // $( '#MinDuration' ).addClass("ui-timepicker-input")
      // $( '#MinDuration' ).timepicker({ 'scrollDefault': 'now', 'timeFormat': 'H:i' });
        $( "#PreferedDate" ).datepicker(dpconfig).datepicker("setDate", new Date());
        $( "#MaxDate" ).datepicker(dpconfig).datepicker("setDate", new Date());
        $( "#MinDuration" ).addClass('ui-timepicker-input').timepicker({ 'scrollDefault': 'now', 'timeFormat': 'H:i'  });
    });
  </script>

<input type="submit">
<% } %>
<pre><%= Html.Encode(ViewData["json"]) %></pre>

</asp:Content>
