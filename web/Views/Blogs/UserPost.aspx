<%@ Page Title="Billet" Language="C#" Inherits="System.Web.Mvc.ViewPage<UUTBlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Model.Title+ " - " + ViewData ["BlogTitle"] ; %>
</asp:Content>
<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1 class="blogtitle"><% if (ViewData["Avatar"]!=null) { %>
<img src="<%=ViewData["Avatar"]%>" alt="" id="logo"/>
<% } %> 
<%= Html.ActionLink(Model.Title,"UserPost","Blog",new{user=Model.UserName, title = Model.Title}) %> 
<span class="c2"> - <%= Html.ActionLink((string)ViewData ["BlogTitle"] ,"UserPosts",new{user=Model.UserName}) %>
</span>
<span class="c3"> - 
<a href="<%=Request.Url.Scheme + "://" + Request.Url.Authority%>"><%= YavscHelpers.SiteName %></a>
</span>
</h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% foreach (var be in Model) { %>
<div class="blogpost">
<%= Html.Markdown(be.Content) %>
</div>
<% if (Membership.GetUser()!=null) { 
	if (Membership.GetUser().UserName==be.UserName)
	 { %> <div class="metapost">
	 <%= Html.ActionLink("Editer","Edit", new { id = be.Id }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","RemovePost", new { id = be.Id }, new { @class="actionlink" } ) %>
	</div> <% } %>
 <% 
		string username = Membership.GetUser().UserName; %>
		<% foreach (var c in (Comment[]) BlogManager.GetComments(be.Id)) {  %> 
<div class="comment" style="min-height:32px;"> <img style="clear:left;float:left;max-width:32px;max-height:32px;margin:.3em;" src="/Blogs/Avatar/<%=c.From%>" alt="<%=c.From%>"/>
<%= Html.Markdown(c.CommentText) %>
	<% if ( username == Model.UserName || c.From == username ) { %>
	<%= Html.ActionLink("Supprimer","RemoveComment", new { cmtid = c.Id } , new { @class="actionlink" })%>
	<% } %>
</div><% } %>
 <div class="postcomment">
	 <% using (Html.BeginForm("Comment","Blogs")) { %>
	 <%=Html.Hidden("UserName")%>
	 <%=Html.Hidden("Title")%>
	 <%=Html.TextArea("CommentText","")%>
	 <%=Html.Hidden("PostId",be.Id)%>
	 <input type="submit" value="Poster un commentaire"/>
	 <% } %>
	  </div>
<% } %><% } %>
</asp:Content>
