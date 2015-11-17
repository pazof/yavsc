<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BasePost>" %>
	 <% if (Membership.GetUser()!=null) { %>
	 <aside>
(<%= Model.Posted.ToString("D") %>
	 - <%= Model.Modified.ToString("D") %> <%= Model.Visible? "":", Invisible!" %>)
	 <%= Html.Partial("RateControl",Model)%>
	 <%
	if (Membership.GetUser().UserName==Model.Author || Roles.IsUserInRole("Admin"))
	 { %>
  <%= Html.Partial("TagControl",Model)%>
<% if (Model is BlogEntry) { %>
<i class="fa fa-pencil"><%=Html.Translate("DoComment")%></i>
 <aside class="control" class="hidden">
	 <% using (Html.BeginForm("Comment","Blogs")) { %>
	 <%=Html.Hidden("Author")%>
	 <%=Html.Hidden("Title")%>
	 <%=Html.TextArea("CommentText","")%>
	 <%=Html.Hidden("PostId",Model.Id)%>
	 <input type="submit" value="Poster un commentaire"/>
	 <% } %>
	  </aside>
<% } %>
	 <a href="<%= Url.RouteUrl("Default", new { action = "EditId", postid = Model.Id })%>" class="actionlink">
	 <i class="fa fa-pencil"><%=Html.Translate("Edit")%></i>
	 </a>
	 <a href="<%= Url.RouteUrl("Default", new { action = "RemovePost", postid = Model.Id })%>" class="actionlink">
	 <i class="fa fa-remove"><%=Html.Translate("Remove")%></i></a>
<% } %>
</aside>
<% } %>
