<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BasePost>" %>
<aside>
(<%= Model.Posted.ToString("yyyy/MM/dd") %>
	 - <%= Model.Modified.ToString("yyyy/MM/dd") %> <%= Model.Visible? "":", Invisible!" %>)
	 <% if (Membership.GetUser()!=null) {
	if (Membership.GetUser().UserName==Model.Author || Roles.IsUserInRole("Admin"))
	 { %>
	 <i class="fa fa-tag"><%=Html.Translate("DoTag")%></i>
	 <a href="<%= Url.RouteUrl("Default", new { action = "Edit", controller = "Blogs", id = Model.Id })%>" class="actionlink">
	 <i class="fa fa-pencil"><%=Html.Translate("Edit")%></i>
	 </a>
	 <a href="<%= Url.RouteUrl("Default", new { action = "RemovePost", controller = "Blogs", id = Model.Id })%>" class="actionlink">
	 <i class="fa fa-remove"><%=Html.Translate("Remove")%></i></a>
<% }} %>
</aside>
