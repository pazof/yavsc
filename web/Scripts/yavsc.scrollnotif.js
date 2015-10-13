//
//  parralax.js
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

$(document).ready(function(){
	var $window = $(window);
	$(window).scroll(function() {
		var $ns = $('#notifications');
		if ($ns.has('*').length>0) {
		if ($window.scrollTop()>375) { 
			$ns.css('position','fixed');
			$ns.css('z-index',2);
			$ns.css('top',0);
		}
		else {  
			$ns.css('position','static');
			$ns.css('z-index',1); 
		}}
	});
});
