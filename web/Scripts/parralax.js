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

	$('[data-type="background"]').each(function(){
		var $bgobj = $(this); // assigning the object
		// get the initial background position, assumes a "X% Yem" ?
		var orgpos = $bgobj.css('backgroundPosition');
		var bgpos = orgpos.split(" ");

		var bgposx = bgpos[0];
		var bgposy = bgpos[1];
		if (/%$/.test(bgposx)){ 
			bgposx = bgposx.substr(0,bgposx.length-1);
			$bgobj.attr('orgbgpxu','% ');
		}
		else if (/em$/.test(bgposx)){
			bgposx = bgposx.substr(0,bgposx.length-2);
			$bgobj.attr('orgbgpxu','em ');
		}
		else if (/px$/.test(bgposx)){
			bgposx = bgposx.substr(0,bgposx.length-2);
			$bgobj.attr('orgbgpxu','px ');
		}
		else { $bgobj.attr('orgbgpxu','px '); } 

		if (/%$/.test(bgposy)){ 
			bgposy = bgposy.substr(0,bgposy.length-1);
			$bgobj.attr('orgbgpyu','% ');
		}
		else if (/em$/.test(bgposy)){
			bgposy = bgposy.substr(0,bgposy.length-2);
			$bgobj.attr('orgbgpyu','em ');
		}
		else if (/px$/.test(bgposy)){
			bgposy = bgposy.substr(0,bgposy.length-2);
			$bgobj.attr('orgbgpyu','px ');
		}
		else { $bgobj.attr('orgbgpyu','px '); } 
		$bgobj.attr('orgbgpx',parseInt(bgposx));
		$bgobj.attr('orgbgpy',parseInt(bgposy));

		$(window).scroll(function() {
		    var speed = $bgobj.data('speed');
		var xPos = $bgobj.attr('orgbgpx') - Math.round($window.scrollLeft() / speed);
		var yPos = $bgobj.attr('orgbgpy') - Math.round($window.scrollTop() / speed);
		// Put together our final background position
		var coords = '' + xPos + $bgobj.attr('orgbgpxu') + yPos + $bgobj.attr('orgbgpyu');
		// Move the background
		$bgobj.css({ backgroundPosition: coords });
		// console.log($bgobj.get()[0].localName+' backgroundPosition: '+coords);
		});
	});
});
