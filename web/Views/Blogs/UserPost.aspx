<%@ Page Title="Billet" Language="C#" Inherits="System.Web.Mvc.ViewPage<UUTBlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Model.Title+ " - " + ViewData ["BlogTitle"] ; %>
</asp:Content>
<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1 class="blogtitle"><% if (ViewData["Avatar"]!=null) { %>
<img src="<%=ViewData["Avatar"]%>" alt="avatar" id="avatar"/>
<% } %> 
<%= Html.ActionLink(Model.Title,"UserPost", new{user=Model.Author, title = Model.Title}, null) %> 
<span> - <%= Html.ActionLink((string)ViewData ["BlogTitle"] ,"UserPosts",new{user=Model.Author}, null) %>
</span>
<span> - 
<a href="<%=Request.Url.Scheme + "://" + Request.Url.Authority%>"><%= YavscHelpers.SiteName %></a>
</span>
</h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% foreach (var be in Model) { %>
<div class="post">
<% if (be.Photo != null) { %>

	<img src="<%=Url.Content(be.Photo)%>" alt="<%=be.Title%>">
<% } %>

<%= Html.Markdown(be.Content,"/bfiles/"+be.Id+"/") %>

 <% string username = Membership.GetUser()==null ? null : Membership.GetUser().UserName; %>
		<% foreach (var c in (Comment[]) BlogManager.GetComments(be.Id)) {  %> 
<div class="comment" style="min-height:32px;"> <img style="clear:left;float:left;max-width:32px;max-height:32px;margin:.3em;" src="<%= Url.Content("~/Account/Avatar/"+c.From) %>" alt="<%=c.From%>"/>
<%= Html.Markdown(c.CommentText) %>
	<% if (Model.Author ==  username || c.From == username ) { %>
	<%= Html.ActionLink("Supprimer","RemoveComment", new { cmtid = c.Id } , new { @class="actionlink" })%>
	<% } %>
</div><% } %>


<% if (Membership.GetUser()!=null) { 
	if (Membership.GetUser().UserName==be.Author)
	 { %> <div class="control">
	 <%= Html.ActionLink("Editer","Edit", new { id = be.Id }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","RemovePost", new { id = be.Id }, new { @class="actionlink" } ) %>
	</div> <% } %>

 <aside class="control">
	 <% using (Html.BeginForm("Comment","Blogs")) { %>
	 <%=Html.Hidden("Author")%>
	 <%=Html.Hidden("Title")%>
	 <%=Html.TextArea("CommentText","")%>
	 <%=Html.Hidden("PostId",be.Id)%>
	 <input type="submit" value="Poster un commentaire"/>
	 <% } %>
	  </aside>
<% } %></div><% } %>
</asp:Content>
