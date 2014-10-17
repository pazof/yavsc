<%@ Page Title="Devis" Language="C#" Inherits="System.Web.Mvc.ViewPage<Estimate>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<script type="text/javascript" src="/js/jquery-latest.js"></script> 
<script type="text/javascript" src="/js/jquery.tablesorter.js"></script> 
<%= Html.ValidationSummary("Devis") %>
<% using  (Html.BeginForm("Estimate","FrontOffice")) { %>
<%= Html.LabelFor(model => model.Title) %>:<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<% if (Model.Id > 0) { %>
<br/>
<%= Html.LabelFor(model => model.Owner) %>:<%=Model.Owner%>
<%= Html.ValidationMessage("Owner", "*") %>
<br/>
<%= Html.LabelFor(model => model.Ciffer) %>:<%=Model.Ciffer%>
<br/>
<%= Html.LabelFor(model => model.Id) %>:<%=Model.Id%>
<%= Html.Hidden( "Id" ) %>
<br/>
<% if (Model.Id==0) { %>
   <input type="submit" name="submit" value="Create"/>
<% } else { %>
   <input type="submit" name="submit" value="Update"/>

   <script type="text/javascript" >

   $(document).ready(function() 
    { 
        $("#myTable").tablesorter( {sortList: [[0,0], [1,0]]} ); 
    } 
);
     </script>

<% } %>
<% if (Model.Lines ==null || Model.Lines.Length == 0) { %>
<i>Pas de ligne.</i>
<%
} else { %>

<table class="tablesorter">
<thead>
<tr>
<th>Id</th>
<th>Description</th>
<th>Product Reference</th>
<th>Count</th>
<th>Unitary Cost</th>
</tr>
</thead>
<tbody id="wrts">
<% foreach (Writting wr in Model.Lines) { %>
<tr>
<td><%=wr.Id%></td>
<td><%=wr.Description%></td>
<td><%=wr.ProductReference%></td>
<td><%=wr.Count%></td>
<td><%=wr.UnitaryCost%></td>
</tr>
<%    } %>
</tbody>
</table>


<%   } %>

<%  } %>

<% } %>

     <script type="text/javascript" >
     function ShowHideBtn(btn,id)
     {
     	var shdiv = document.getElementById(id);
     	var wanit = shdiv.style.display == "none"; // switch button
     	shdiv.style.display = wanit ? "block" : "none";
     	btn.value = wanit ? "-" : "+";
     }
     </script>

    <form id="writeform">
  <input type="button" onclick="ShowHideBtn(this,'writearea');" value="+"/>
  <div id="writearea" style="display:none;">
     <input type="hidden" name="estid" id="estid" value="<%=Model.Id%>"/>
     <label for="desc">Description:</label>
     <input type="text" name="desc" id="desc" />
     <label for="ucost">Prix unitaire:</label>
     <input type="number" name="ucost" id="ucost"/>
     <label for="count">Quantité:</label>
     <input type="number" name="count" id="count"/>
     <label for="productid">Référence du produit:</label>
     <input type="text" name="productid" id="productid"/> 
     <input type="button" name="btnapply" id="btnapply" value="Écrire"/>
     <input type="hidden" name="wrid" id="wrid" />
     <tt id="dbgv"></tt>
     <input type="button" name="btnview" id="btnview" value="View Values"/>

     <script>

 jQuery.support.cors = false;  
     function GetWritting () {
		var estid = parseInt($("#estid").val());
    	var ucost = Number($("#ucost").val());
    	var count = parseInt($("#count").val());
    	var desc = $("#desc").val();
    	var productid = $("#productid").val();
     	return {
     	estid:estid,
     	desc:desc,
     	ucost:ucost,
     	count:count,
     	productid:productid
     	};
     }
        

     $(document).ready(function () {

 $("#btnapply").click(function () {
    	
        $.ajax({
            url: "http://localhost:8080/api/WorkFlow/Write",
            type: "Get",
            dataType: "json",
            data: GetWritting(), 
            contentType: 'application/json; charset=utf-8',
            success: function (data) { 
            $("#wrid").val(data);
            },
            error: function (xhr, ajaxOptions, thrownError) {
        $("#dbgv").text(xhr.status+" : "+xhr.responseText);

      }
        });
    });

    $("#btnview").click(function () {
    	$("#dbgv").text(JSON.stringify(GetWritting()));
    });
});
    </script>
  </div>
</form>
</asp:Content>


