<%@ Page Title="Blogs - Les dernières parutions" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<% foreach (BlogEntry e in this.Model) { %>
<div class="blogpost">
<h3 class="blogtitle">
<%= Html.ActionLink(e.Title,"UserPost", new { user=e.UserName, title = e.Title }) %>
</h3>
	<div class="metablog">(<%=Html.Encode(e.UserName)%> <%=e.Modified.ToString("yyyy/MM/dd") %>)
<% if (Membership.GetUser()!=null)
	if (Membership.GetUser().UserName==e.UserName)
	 { %>
	 <%= Html.ActionLink("Editer","Edit", new { user = e.UserName, title = e.Title }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","RemovePost", new { user = e.UserName, title = e.Title }, new { @class="actionlink" } ) %>
	 <% } %>
	 </div>
</div>

<% } %>
	  <form runat="server" id="form1" method="GET">
	<% rp1.ResultCount = (int) ViewData["RecordCount"];
 rp1.CurrentPage = (int) ViewData["PageIndex"]; %>
	 <yavsc:ResultPages id="rp1" Action = "?pageIndex={0}" runat="server" ></yavsc:ResultPages> 
 		
	 </form>
</asp:Content>

