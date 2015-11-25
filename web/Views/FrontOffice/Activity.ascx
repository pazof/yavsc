<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Activity>" %>

<h1 class="activity">
<%=Html.Icon("act"+Model.Id)%>
<%=Html.Markdown(Model.Title)%></h1>
<i>(<%=Html.Markdown(Model.Id)%>)</i>
<p>
<%=Html.Markdown(Model.Comment)%>
</p>
