# TA.Utilities Overview

This library represents a collection of classes factored out of our production projects that we found were being used over and over again. Rather than re‑using the code at source level, it is collected together in this package as a general‑purpose reusable library and made freely available for you to use at no cost and with no obligation. The only stipulation is that you cannot sue the author or Timtek Systems Limited if anything bad happens as a result of you using the code. It is up to you to determine suitability for your purpose.

## Software re‑use at the object code level

This was always the promise of object‑oriented design, but it was not until the advent of [NuGet][nuget] and its widespread adoption that this became a practical reality. It is easy to overlook the impact of [NuGet][nuget], as it seems so obvious and natural once you have used it.

> "Dependency management is the key challenge in software at every scale." — possibly attributed to **Donald Knuth**, _The Art of Computer Programming_

NuGet has essentially solved a large chunk of the dependency management problem. At Timtek Systems, we use NuGet as a key component in our software design strategy. We publish our open‑source code on a [public MyGet feed][myget]. We push both prerelease and release versions to [MyGet][myget]. When we make an official release, we promote that package from [MyGet][myget] to [NuGet][nuget]. You can consume our packages from either location, but if you want betas and release candidates, you will need to use [our MyGet feed][myget].

## Licensing

This software is released under the [Tigra MIT Licence][mit], which (in summary) means: "Anyone can do anything at all with this software without limitation, but it is not our fault if anything goes wrong".

Our [philosophy of open source][yt-oss] is to [give wholeheartedly with no strings attached][yt-oss]. We have no time for “copyleft” licences which we find irksome. So here it is, for you to use however you like, no strings attached.

I tend to use “we” and “our” when talking about the company, but Timtek Systems Limited is a one‑man operation run by me, Tim Long. I hope you find the software useful, and if you feel that my efforts are worth supporting, then it would make my day if you would [buy me some coffee][coffee]. I also would not mind you giving us a mention, if you feel you are able to, as it helps the company grow. Donations and mentions really make a difference, so please think about it and do what you can.

If you are a company and need some work done, then consider hiring me as a freelance developer. I have decades of experience in product design, firmware development for embedded systems and PC driver and software development. I am a professional; I believe in doing what is right, not what is expedient, and I support my software.

See also: [[Release Notes]].

[nuget]: https://www.nuget.org/ "NuGet gallery"
[myget]: https://www.myget.org/feed/Packages/tigra-astronomy "Tigra Astronomy public package feed"
[mit]: https://tigra.mit-license.org "Tigra MIT Licence"
[yt-oss]: https://www.youtube.com/watch?v=kloweL2fw7Q "Set your software free"
[coffee]: https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=ARU8ANQKU2SN2&source=url "Support our open source projects with a donation"
