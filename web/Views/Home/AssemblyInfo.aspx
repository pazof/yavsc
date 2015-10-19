<%@ Page Title="Assemblies" Language="C#" Inherits="System.Web.Mvc.ViewPage<IEnumerable<System.Reflection.AssemblyName>>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="panel">
<p>
Running assembly :
<%= GetType().Assembly.FullName %></p>
</div>
<div class="panel">
<p>
Assemblies referenced in this application :
</p>
<ul>
<% foreach (System.Reflection.AssemblyName item in Model) { %>
<li><%= item.FullName %></li>
<% } %>
</ul>
</div>
 </asp:Content>

