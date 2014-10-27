<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEditEntryModel>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="head" ID="head1" runat="server" >
<script type="text/javascript" src="/js/jquery-latest.js"></script> 
<script type="text/javascript" src="/js/rangyinputs-jquery-1.1.2.js"></script> 
</asp:Content>
<asp:Content ContentPlaceHolderID="overHeaderOne" ID="headerContent" runat="server">	
<h1 class="blogtitle"> 
	<a href="/Blog/<%=ViewData["UserName"]%>">
 <img class="avatar" src="/Blogs/Avatar?user=<%=ViewData["UserName"]%>" alt="from <%=ViewData["UserName"]%>"/>
 <%= Html.Encode(ViewData["BlogTitle"]) %> </a> - 
Nouveau billet -
<a href="/"><%=  Html.Encode(YavscHelpers.SiteName) %></a> </h1>

</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="message">
	<%= Html.Encode(ViewData["Message"]) %>
</div>
	<% if (Model != null ) if (Model.Content != null )  { %>
<div class="blogpost">
<%= BBCodeHelper.Parser.ToHtml(Model.Content) %>
</div>
<% } %>
	<div class="editpost">


<%= Html.ValidationSummary() %>
<% using(Html.BeginForm("ValidatePost", "Blogs")) %>
<% { %>

<%= Html.LabelFor(model => model.Title) %>:<br/>
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<br/>
<div>
	Usage BBcodes  :
	 <ul>
<% foreach (string usage in BBCodeHelper.BBTagsUsage) { %>
<li><%= usage %></li>
<% } %>
	</ul>

</div>
<%= Html.LabelFor(model => model.Content) %>:<br/>
<%= Html.TextArea( "Content", new { @class="editblog", @rows="15" }) %>
<%= Html.ValidationMessage("Content", "*") %>
<br/>
<%= Html.CheckBox( "Visible" ) %>
<%= Html.LabelFor(model => model.Visible) %>
<%= Html.ValidationMessage("Visible", "*") %>
<br/>
<%= Html.CheckBox( "Preview" ) %> <%= Html.LabelFor(model => model.Preview) %>
<%= Html.ValidationMessage("Preview", "*") %>
<br/>
	<%= Html.Hidden("Id") %>
	<input type="button" id="btnpreview" value="<%=LocalizedText.Preview %>"/>
<input type="submit"/>
	<input type="button" id="testbtn">
	<script language="Javascript">
	$(document).ready(function () {
		$("#testbtn").click(function () {
			$("#Content").replaceSelectedText("SOME NEW TEXT");
		});
	});
	</script>
<% } %>

</div>
</asp:Content>

