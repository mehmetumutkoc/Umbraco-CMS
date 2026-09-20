  /*
  |--------------------------------------------------------------------------
  | Template Name: Portnew
  | Author: Sifency
  | Version: 1.0.0
  |--------------------------------------------------------------------------
  |--------------------------------------------------------------------------
  | TABLE OF CONTENTS:
  |--------------------------------------------------------------------------
  |
  | 1. Scripts initialization
  | 2. Preloader
  | 3. Sticky Header
  | 4. Mobile Menu
  | 5. Text Marquee
  | 6. Testimonial Slider
  | 7. Client Slider
  | 8. Video Button
  | 9. Scroll To Top
    10. Parallax Swiper Slider
    11. Custom Cursor
  */
 

   /*--------------------------------------------------------------
    1. Scripts initialization
  --------------------------------------------------------------*/
  $.exists = function (selector) {
    return $(selector).length > 0;
  };

  (function () {
    if (!window.Swiper) {
      return;
    }
    var OriginalSwiper = window.Swiper;
    window.Swiper = function (el, opts) {
      if (typeof el === "string" && !document.querySelector(el)) {
        return null;
      }
      return new OriginalSwiper(el, opts);
    };
    window.Swiper.prototype = OriginalSwiper.prototype;
  })();

  $(window).on('load', function () {
    $(window).trigger('scroll');
    $(window).trigger('resize');
  });

  $(function () {
    $(window).trigger('resize');
    mainNav();
    stickyHeader();
    if ($.exists('.wow')) {
      new WOW().init();
    }
  });
  AOS.init();

  /*--------------------------------------------------------------
    2. Preloader
  --------------------------------------------------------------*/
  window.addEventListener("load", function () {
    const loader = document.querySelector(".loader");
    if (loader) {
      loader.className += " hidden";
    }
    });
  /*--------------------------------------------------------------
    03. Sticky Header
  --------------------------------------------------------------*/
  function stickyHeader() {
    var $window = $(window);
    var lastScrollTop = 0;
    var $header = $('.cs_sticky_header');
    var headerHeight = $header.outerHeight() + 30;

    $window.scroll(function () {
      var windowTop = $window.scrollTop();

      if (windowTop >= headerHeight) {
        $header.addClass('cs_gescout_sticky');
      } else {
        $header.removeClass('cs_gescout_sticky');
        $header.removeClass('cs_gescout_show');
      }

      if ($header.hasClass('cs_gescout_sticky')) {
        if (windowTop < lastScrollTop) {
          $header.addClass('cs_gescout_show');
        } else {
          $header.removeClass('cs_gescout_show');
        }
      }

      lastScrollTop = windowTop;
    });
  }
  /*--------------------------------------------------------------
    04. Mobile Menu
  --------------------------------------------------------------*/
  function mainNav() {
    $('.cs_nav').append('<span class="cs_munu_toggle"><span></span></span>');
    $('.menu-item-has-children').append(
      '<span class="cs_munu_dropdown_toggle"></span>',
    );
    $('.cs_munu_toggle').on('click', function () {
      $(this)
        .toggleClass('cs_toggle_active')
        .siblings('.cs_nav_list')
        .slideToggle();
    });
    $('.cs_munu_dropdown_toggle').on('click', function () {
      $(this).toggleClass('active').siblings('ul').slideToggle();
      $(this).parent().toggleClass('active');
    });
    // Mega Menu
    // $('.cs_mega_wrapper>li>a').removeAttr('href');
    // Modal Btn
    $('.cs_mode_btn').on('click', function () {
      $(this).toggleClass('active');
      $('body').toggleClass('cs_dark');
    });
    // Side Nav
    $('.cs_icon_btn').on('click', function () {
      $('.cs_side_header').addClass('active');
    });
    $('.cs_close, .cs_side_header_overlay').on('click', function () {
      $('.cs_side_header').removeClass('active');
    });
    //  Menu Text Split
    $('.cs_animo_links > li > a').each(function () {
      let xxx = $(this).html().split('').join('</span><span>');
      $(this).html(`<span class="cs_animo_text"><span>${xxx}</span></span>`);
    });
  };

  /*--------------------------------------------------------------
    05. Text Marquee
  --------------------------------------------------------------*/
  $(document).on('DOMContentLoaded', function() {
    const marqueeContent = $('ul.marquee-content');
    const marqueeElementsDisplayed = getComputedStyle(document.documentElement).getPropertyValue("--marquee-elements-displayed");
  
    document.documentElement.style.setProperty("--marquee-elements", marqueeContent.children().length);
  
    for (let i = 0; i < marqueeElementsDisplayed; i++) {
      marqueeContent.append(marqueeContent.children().eq(i).clone());
    }
  });

  /*--------------------------------------------------------------
    06. Testimonial slider
  --------------------------------------------------------------*/
  $(document).ready(function(){
    $('.slider').slick({
      prevArrow: '.testi-previous-btn',
      nextArrow: '.testi-next-btn',
      autoplay: true,
      dots: false,
      arrows: true
    });
  });

  /*--------------------------------------------------------------
    07. Client slider
  --------------------------------------------------------------*/
  $(document).ready(function(){
    $('.carousel').slick({
      prevArrow: '.client-previous-btn',
      nextArrow: '.client-next-btn',
      autoplay: true,
      dots: false,
      arrows: true,
      infinite: true,
      speed: 500,
      slidesToShow: 5,
      slidesToScroll: 1,
      responsive: [
        {
          breakpoint: 992,
          settings: {
            slidesToShow: 2
          }
        },
        {
          breakpoint: 768,
          settings: {
            slidesToShow: 1
          }
        }
      ]
    });
  });

  /*--------------------------------------------------------------
    08. Video Play Button
  --------------------------------------------------------------*/

$('#play-video').on('click', function(e){
  e.preventDefault();
  $('#video-overlay').addClass('open');
  $("#video-overlay").append('<iframe width="1182" height="731" src="https://www.youtube.com/embed/l-epKcOA7RQ" title="The Envato Story | Inside Envato" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>');
});

$('.video-overlay, .video-overlay-close').on('click', function(e){
  e.preventDefault();
  close_video();
});

$(document).keyup(function(e){
  if(e.keyCode === 27) { close_video(); }
});

function close_video() {
  $('.video-overlay.open').removeClass('open').find('iframe').remove();
};


  /*--------------------------------------------------------------
    09. Scroll To Top
  --------------------------------------------------------------*/
const toTop = document.querySelector(".to-top");

window.addEventListener("scroll", () => {
  if (window.pageYOffset > 50) {
    toTop.classList.add("active");
  } else {
    toTop.classList.remove("active");
  }
});

/*--------------------------------------------------------------
    10. Parallax Swiper Slider
  --------------------------------------------------------------*/

  var swiper = new Swiper(".cs-swiper", {
    slidesPerView: 3,
    spaceBetween: 20,
    freeMode: true,
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-button-next",
      prevEl: ".swiper-button-prev",
    },
    breakpoints: {
      1320: {
        slidesPerView: 3,
        spaceBetween: 25,
      },
      1024: {
        slidesPerView: 3,
        spaceBetween: 20,
      },
      991: {
        slidesPerView: 2,
        spaceBetween: 20,
      },
      767: {
        slidesPerView: 2,
        spaceBetween: 15,
      },
      550: {
        slidesPerView: 2,
        spaceBetween: 10,
      },
      400: {
        slidesPerView: 1,
      },
      399: {
        slidesPerView: 1,
      },
      350: {
        slidesPerView: 1,
      },
      0: {
        slidesPerView: 1,
      }
    },
  });
  var swiper = new Swiper(".cs-swiper-blog", {
    slidesPerView: 4,
    spaceBetween: 20,
    freeMode: true,
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-button-next-blog",
      prevEl: ".swiper-button-prev-blog",
    },
    breakpoints: {
      1320: {
        slidesPerView: 4,
        spaceBetween: 25,
      },
      1024: {
        slidesPerView: 3,
        spaceBetween: 20,
      },
      991: {
        slidesPerView: 2,
        spaceBetween: 20,
      },
      767: {
        slidesPerView: 2,
        spaceBetween: 15,
      },
      550: {
        slidesPerView: 2,
        spaceBetween: 10,
      },
      400: {
        slidesPerView: 1,
      },
      399: {
        slidesPerView: 1,
      },
      350: {
        slidesPerView: 1,
      },
      0: {
        slidesPerView: 1,
      }
    },
  });


  var swiper = new Swiper(".cs-swiper-portfolio", {
    slidesPerView: 4,
    spaceBetween: 30,
    freeMode: true,
    pagination: {
      el: ".swiper-pagination-portfolio",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-button-next-portfolio",
      prevEl: ".swiper-button-prev-portfolio",
    },
    breakpoints: {
      1320: {
        slidesPerView: 4,
        spaceBetween: 25,
      },
      1024: {
        slidesPerView: 3,
        spaceBetween: 20,
      },
      991: {
        slidesPerView: 2,
        spaceBetween: 20,
      },
      767: {
        slidesPerView: 2,
        spaceBetween: 15,
      },
      550: {
        slidesPerView: 2,
        spaceBetween: 10,
      },
      400: {
        slidesPerView: 1,
      },
      399: {
        slidesPerView: 1,
      },
      350: {
        slidesPerView: 1,
      },
      0: {
        slidesPerView: 1,
      }
    },
  });


  var swiper = new Swiper(".cs-swiper-team", {
    slidesPerView: 4,
    spaceBetween: 30,
    freeMode: true,
    pagination: {
      el: ".swiper-pagination-team",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-button-next-team",
      prevEl: ".swiper-button-prev-team",
    },
    breakpoints: {
      0: {
        slidesPerView: 1,
      },
      350: {
        slidesPerView: 1,
      },
      399: {
        slidesPerView: 1,
      },
      400: {
        slidesPerView: 1,
      },
      550: {
        slidesPerView: 2,
        spaceBetween: 10,
      },
      767: {
        slidesPerView: 2,
        spaceBetween: 15,
      },
      991: {
        slidesPerView: 2,
        spaceBetween: 20,
      },
      1024: {
        slidesPerView: 3,
        spaceBetween: 20,
      },
      1320: {
        slidesPerView: 4,
        spaceBetween: 25,
      }
    },
  });
  var swiper = new Swiper(".cs-swiper-testi", {
    slidesPerView: 2,
    spaceBetween: 20,
    freeMode: true,
    pagination: {
      el: ".swiper-pagination-testi",
      clickable: true,
    },
    navigation: {
      nextEl: ".swiper-button-next-testi",
      prevEl: ".swiper-button-prev-testi",
    },
    breakpoints: {
      0: {
        slidesPerView: 1,
      },
      350: {
        slidesPerView: 1,
      },
      399: {
        slidesPerView: 1,
      },
      400: {
        slidesPerView: 1,
      },
      550: {
        slidesPerView: 2,
        spaceBetween: 10,
      },
      767: {
        slidesPerView: 2,
        spaceBetween: 15,
      },
      991: {
        slidesPerView: 2,
        spaceBetween: 20,
      },
      1024: {
        slidesPerView: 2,
        spaceBetween: 20,
      },
      1320: {
        slidesPerView: 2,
        spaceBetween: 25,
      }
    },
  });


    /*--------------------------------------------------------------
    11. Cursor Animation
  --------------------------------------------------------------*/
  document.addEventListener('DOMContentLoaded', () => {
    if (!window.Cursor || document.body.classList.contains('fv-no-cursor') || window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      return;
    }
    new window['Cursor']({
      count: 2,
      targets: ['a'],
    })
  });
  
