<%@ Page Title="Blog" Language="C#" Inherits="System.Web.Mvc.ViewPage<UUBlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = (string) ViewData ["BlogTitle"] ; %>
</asp:Content>

<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">

<% if (!string.IsNullOrEmpty((string)ViewData["Avatar"])) { %>
<a href="<%=Url.Content("~/Blog/"+Model.Author)%>" id="avatar">
<img src="<%=ViewData["Avatar"]%>" />
</a>
<% } %>
<h1 class="blogtitle">
<a href="<%=Url.Content("~/Blog/"+Model.Author)%>">
<%=Html.Encode(ViewData["BlogTitle"])%></a>
- <a href="<%= Url.Content("~/") %>"><%= YavscHelpers.SiteName %></a>
</h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%  foreach (BlogEntry e in this.Model) { %>
<div class="postpreview<% if (!e.Visible) { %> hiddenpost<% } %>" >
<h2><%= Html.ActionLink(e.Title,"UserPost", new { user=e.Author, title=e.Title, id = e.Id }, new { @class = "usertitleref" }) %></h2>
<% bool truncated = false; %>
<%= Html.MarkdownToHtmlIntro(out truncated, e.Content,"/bfiles/"+e.Id+"/") %>
<% if (truncated) { %>
  <i><%= Html.ActionLink( "lire la suite" ,"UserPost", new { user=e.Author, title=e.Title, id = e.Id }, new { @class = "usertitleref" }) %></i>
  <% } %>
<aside>(<%= e.Posted.ToString("yyyy/MM/dd") %>
	 - <%= e.Modified.ToString("yyyy/MM/dd") %> <%= e.Visible? "":", Invisible!" %>)
	 <% if (Membership.GetUser()!=null)
	if (Membership.GetUser().UserName==e.Author)
	 { %>
	 <%= Html.ActionLink("Editer","Edit", new { id = e.Id }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","RemovePost", new { id = e.Id }, new { @class="actionlink" } ) %>
	 <% } %>


	 </aside>
</div>
<% } %>
<aside>
	<form runat="server" id="form1" method="GET">
<%
 rp1.ResultCount = (int) ViewData["RecordCount"];
 rp1.PageIndex = (int) ViewData["PageIndex"];
%>
<yavsc:ResultPages id="rp1" Action = "?pageIndex={0}" runat="server">
	 <None><i>Pas de contenu</i></None>
</yavsc:ResultPages> 
	</form>
</aside>
	

</asp:Content>
