<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BasePost>" %>
<aside>
(<%= Model.Posted.ToString("D") %>
	 - <%= Model.Modified.ToString("D") %> <%= Model.Visible? "":", Invisible!" %>)
	 <% if (Membership.GetUser()!=null) {
	if (Membership.GetUser().UserName==Model.Author || Roles.IsUserInRole("Admin"))
	 { %>
<% if (Model is BlogEntry) { %><%= Html.Partial("TagControl",Model)%><% } %>
	 <a href="<%= Url.RouteUrl("BlogById", new { action = "Edit", postid = Model.Id })%>" class="actionlink">
	 <i class="fa fa-pencil"><%=Html.Translate("Edit")%></i>
	 </a>
	 <a href="<%= Url.RouteUrl("BlogById", new { action = "RemovePost", postid = Model.Id })%>" class="actionlink">
	 <i class="fa fa-remove"><%=Html.Translate("Remove")%></i></a>
<% }} %>
</aside>
