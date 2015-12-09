<%@ Page Title="Catalog" Language="C#"  Inherits="System.Web.Mvc.ViewPage<ProductCategory>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% foreach (Product p in Model.Products ) { %>

 	<h3><%= Html.TranslatedActionLink( p.Name, "Product", new { id = ViewData["BrandId"], pc = Model.Reference , pref = p.Reference }, new { @class="actionlink" } ) %></h3>

 	<p>
 	<%= p.Description %>
 		<% if (p.Images !=null)
 		 foreach (ProductImage i in p.Images ) { %>
 		<img src="<%=i.Src%>" alt="<%=i.Alt%>"/>
 		<% } %>
 	</p>


 	<% } %>

	
</asp:Content>
