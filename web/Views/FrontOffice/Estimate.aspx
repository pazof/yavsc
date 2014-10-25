<%@ Page Title="Devis" Language="C#" Inherits="System.Web.Mvc.ViewPage<Estimate>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="head" ID="head1" runat="server" >
<script type="text/javascript" src="/js/jquery-latest.js"></script> 
<script type="text/javascript" src="/js/jquery.tablesorter.js"></script> 
<link rel="stylesheet" href="/Theme/dark/style.css" type="text/css" media="print, projection, screen" />

</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<%= Html.ValidationSummary("Devis") %>
<% using  (Html.BeginForm("Estimate","FrontOffice")) { %>
<%= Html.LabelFor(model => model.Title) %>:<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<br/>
<%= Html.LabelFor(model => model.Responsible) %>:<%=Model.Responsible%>
<%= Html.Hidden ("Responsible") %>
<%= Html.ValidationMessage("Responsible", "*") %>
<br/>
<%= Html.LabelFor(model => model.Client) %>:<%=Html.TextBox( "Client" ) %>
<%= Html.ValidationMessage("Client", "*") %>
<br/>
<%= Html.LabelFor(model => model.Description) %>:<%=Html.TextArea( "Description") %>
<%= Html.ValidationMessage("Description", "*") %>
<br/>
<%= Html.LabelFor(model => model.Id) %>:<%=Model.Id%>
<%= Html.Hidden( "Id" ) %>
<br/>
<% if (Model.Id==0) { %>
   <input type="submit" name="submit" value="Create"/>
<% } else { %>
   <input type="submit" name="submit" value="Update"/>

<% } %>


   <% if (Model.Id>0) { %>

<table class="tablesorter">
<thead>
<tr>
<th>Description</th>
<th>Product Reference</th>
<th>Count</th>
<th>Unitary Cost</th>
</tr>
</thead>
<tbody id="wrts">
<% int lc=0;
   if (Model.Lines!=null)
   foreach (Writting wr in Model.Lines) { lc++; %>
<tr class="<%= (lc%2==0)?"odd ":"" %>row" id="wr<%=wr.Id%>">
<td><%=wr.Description%></td>
<td><%=wr.ProductReference%></td>
<td><%=wr.Count%></td>
<td><%=wr.UnitaryCost%></td>
</tr>
<%    } %>
</tbody>
</table>

<%  } %>

<% } %>

   </asp:Content>
   <asp:Content ContentPlaceHolderID="MASContent" ID="MASContent1" runat="server">


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
      <input type="button" id="btndtl" value="+"/>
     
     <div id="writearea" style="display:none;">
     <input type="button" id="btnnew" value="Nouvelle écriture"/>
     <input type="hidden" name="estid" id="estid" value="<%=Model.Id%>"/>
     <label for="Description">Description:</label>
     <input type="text" name="Description" id="Description" />
     <label for="UnitaryCost">Prix unitaire:</label>
     <input type="number" name="UnitaryCost" id="UnitaryCost" step="0.01"/>
     <label for="Count">Quantité:</label>
     <input type="number" name="Count" id="Count"/>
     <label for="ProductReference">Référence du produit:</label>
     <input type="text" name="ProductReference" id="ProductReference"/> 
     <input type="button" name="btnmodify" id="btnmodify" value="Modifier"/>
     <input type="button" name="btncreate" id="btncreate" value="Écrire"/>
     <input type="button" name="btndrop" id="btndrop" value="Supprimer"/>
     <input type="hidden" name="wrid" id="wrid" />

     <tt id="msg" class="message"></tt>
     <style>
     .row { cursor:pointer; }
     table.tablesorter td:hover { background-color: rgba(0,64,0,0.5); }
     .hidden { display:none; }
     .selected *, .odd .selected { font-size-adjust: +1; background-color: rgb(0,64,0); }
    
</style>
     <script>

 jQuery.support.cors = true;  
     function GetWritting () {
     	return {
     	Id: Number($("#wrid").val()),
     	UnitaryCost: Number($("#UnitaryCost").val()),
     	Count: parseInt($("#Count").val()),
     	ProductReference: $("#ProductReference").val(),
     	Description: $("#Description").val()
     	};
     }

     function wredit(pwrid)
     {

    	$("#wr"+wrid.value).removeClass("selected");
     	$("#wrid").val(pwrid);
     	if (wrid.value!=0)
    		$("#wr"+wrid.value).addClass("selected");
     	if (pwrid>0) {
    	$("#btncreate").addClass("hidden");
    	$("#btnmodify").removeClass("hidden");
    	$("#btndrop").removeClass("hidden");
    	} else {
    	$("#btncreate").removeClass("hidden");
    	$("#btnmodify").addClass("hidden");
    	$("#btndrop").addClass("hidden");
    	}
     } 

     function delRow() {
       $.ajax({
            url: "<%=ViewData["WebApiUrl"]+"/DropWritting"%>",
            type: "Get",
            data: { wrid: wrid.value }, 
            contentType: 'application/json; charset=utf-8',
            success: function () { 
		       var tr = document.getElementById("wr"+wrid.value);
		       $("#wr"+wrid.value).remove();
		       $("#wrid").val(0);
		    	$("#ucost").val(0);
		    	$("#Count").val(0);
		    	$("#Description").val();
		    	$("#ProductReference").val();
			   wredit(0);
            },
            error: function (xhr, ajaxOptions, thrownError) {
        $("#msg").text(xhr.status+" : "+xhr.responseText);}
        });

      }
	
	function setRow() {
       var wrt = GetWritting();

       $.ajax({
            url: "<%=ViewData["WebApiUrl"]+"/UpdateWritting"%>",
            type: "POST",
            data: JSON.stringify(wrt), 
            contentType: 'application/json; charset=utf-8',
            success: function () { 
		       var cells = document.getElementById("wr"+wrt.Id).getElementsByTagName("TD");
		       cells[0].innerHTML=wrt.Description;
		       cells[1].innerHTML=wrt.ProductReference;
		       cells[2].innerHTML=wrt.UnitaryCost;
		       cells[3].innerHTML=wrt.Count;
            },
            error: function (xhr, ajaxOptions, thrownError) {
        $("#msg").text(xhr.status+" : "+xhr.responseText);}
        });
    }


function addRow(){
       var wrt = GetWritting();
		var estid = parseInt($("#Id").val());
        $.ajax({
            url: "<%=ViewData["WebApiUrl"]+"/Write"%>/?estid="+estid,
            type: "POST",
            traditional: false,
            data: JSON.stringify(wrt),
            contentType: 'application/json; charset=utf-8',
            success: function (data) { 
            wrt.Id = Number(data);
            wredit(wrt.Id);
            $("<tr class=\"row\" id=\"wr"+wrt.Id+"\"><td>"+wrt.Description+"</td><td>"+wrt.ProductReference+"</td><td>"+wrt.Count+"</td><td>"+wrt.UnitaryCost+"</td></tr>").appendTo("#wrts");
             },
            error: function (xhr, ajaxOptions, thrownError) {
        $("#msg").text(xhr.status+" : "+xhr.responseText);}
        });
        $(".wr"+wrt.Id).click(onEditRow(e));  
    }

    function ShowDtl(val)
     {
     	document.getElementById("writearea").style.display = val ? "block" : "none";
     	document.getElementById("btndtl").value = val ? "-" : "+";
     }

     function onEditRow(e) {
    	ShowDtl(true);
    	var cells = e.delegateTarget.getElementsByTagName("TD");
    	var hid=e.delegateTarget.id;
    	var vwrid = Number(hid.substr(2));
    	wredit(vwrid);
    	$("#Description").val(cells[0].innerHTML);
    	$("#ProductReference").val(cells[1].innerHTML);
    	$("#Count").val(cells[2].innerHTML);
    	$("#UnitaryCost").val(Number(cells[3].innerHTML.replace(",",".")));
    }
  
 $(document).ready(function () {
     // bug when no row: $(".tablesorter").tablesorter( {sortList: [[0,0], [1,0]]} ); 
        
    $("#btncreate").click(addRow);
    $("#btnmodify").click(setRow);
    $("#btndrop").click(delRow);
    $("#btndtl").click(function(){ShowDtl(document.getElementById("writearea").style.display != "block");});
    $(".row").click(function (e) {onEditRow(e);});
	$("#btnnew").click(function () {
		wredit(0);
		$("#Description").val("");
    	$("#ProductReference").val("");
    	$("#Count").val(1);
    	$("#UnitaryCost").val(0);
	});
});
    </script>
  </div>
    </form>
</asp:Content>


