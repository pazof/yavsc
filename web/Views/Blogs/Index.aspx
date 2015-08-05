<%@ Page Title="Blogs - Indexe" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master" EnableTheming="True" StylesheetTheme="dark" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<div>
<% foreach (var g in Model.GroupByUser()) { %>
<h1><a href="<%= Url.Content("~/Blog") %>" class="actionlink userref">
<%=g.Key%></a></h1>
<% foreach (var p in g) { %> 
<div class="blogpost">


<%= Html.ActionLink(p.Title, "UserPost",
 new { user = g.Key, title = p.Title }, new { @class = "usertitleref" } ) %>
le  <%=p.Posted.ToString("D") %>

  <% if (Membership.GetUser()!=null)
	if ((Membership.GetUser().UserName==g.Key)
	|| (Roles.IsUserInRole ("Admin")))
	 { %><aside>
	 <%= Html.ActionLink("Editer","Edit", new { id = p.Id }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","RemovePost", new { id = p.Id }, new { @class="actionlink" } ) %>
</aside><% } %>
</div> <% } %>
 <% } %>
</div>

	  <form runat="server" id="form1" method="GET">
	<% rp1.ResultCount = Model.Count; rp1.ResultsPerPage = 50; %>
<% rp1.CurrentPage = (int) ViewData["PageIndex"]; %>
	 <yavsc:ResultPages id="rp1" Action = "?pageIndex={0}" runat="server" ></yavsc:ResultPages> 
 		
	 </form>
</asp:Content>

