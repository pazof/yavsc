<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Activity>" %>
<div>
<div data-type="background" data-speed="10" style="width:100%; height:10em; background: url(<%=Ajax.JString(Model.Photo)%>) 50% 50% repeat fixed;" >
</div>
<h1><a href="<%= Url.RouteUrl("Default", new { controller="FrontOffice", action="Booking", id=Model.Id }) %>">
<%=Html.Encode(Model.Title)%></a></h1>
<i>(<%=Html.Translate("MEACode")%>: <%=Html.Encode(Model.Id)%>)</i>
<p>
<%=Html.Markdown(Model.Comment)%>
</p></div>
