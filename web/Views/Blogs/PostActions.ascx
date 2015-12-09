<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BasePost>" %>
	 <% if (Membership.GetUser()!=null) { %>
	 <aside>
(<%= Html.Translate("Posted") %> <%= Model.Posted.ToString("D") %>
	 - <%= Html.Translate("Edited") %> <%= Model.Modified.ToString("D") %> <%= Model.Visible? "":", Invisible!" %>)
	 <%= Html.Partial("RateControl",Model)%>
	 <%
	if (Membership.GetUser().UserName==Model.Author || Roles.IsUserInRole("Admin"))
	 { %>
  <%= Html.Partial("TagControl",Model)%>
<% if (Model is BlogEntry) { %>
	 <% using (Html.BeginForm("Comment","Blogs")) { %>
	 <fieldset>
	 <legend><i class="fa fa-pencil"></i><%=Html.Translate("DoComment")%></legend>
	<div> <%=Html.Hidden("Author")%>
	 <%=Html.Hidden("Title")%>
	 <%=Html.TextArea("CommentText","")%>
	 <%=Html.Hidden("PostId",Model.Id)%>
	 <input type="submit" value="Poster un commentaire"/></div>
	 </fieldset> <% } %>
<% } %>
	 <a href="<%= Url.RouteUrl("Default", new { action = "EditId", postid = Model.Id })%>" class="actionlink">
	 <i class="fa fa-pencil"><%=Html.Translate("Edit")%></i>
	 </a>
	 <a href="<%= Url.RouteUrl("Default", new { action = "RemovePost", postid = Model.Id })%>" class="actionlink">
	 <i class="fa fa-remove"><%=Html.Translate("Remove")%></i></a>
<% } %>
</aside>
<% } %>
