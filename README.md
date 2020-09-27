# datahawk

**SOME FUN INTO AMAZON SCRAPPING**

At first, I started a new angular project with the dotnet cli default code for starting a new Angular app

But because I'm not initially a fullstack or front-end developer, and definitely not an Angular developer, I decided to focus on the scrapping part.
(You can see I've had some React.Js training recently (2020) but not Angular :))

As a consequence, and because I don't have a unlimited time to do this exercise which already took me a little while, I've canceled my wish to try an angular interface.
I may give it a try later without the clock tick on my back.

**HOW TO START**
To start the app, simple : cd amazon_scraper && dotnet run .

**ENDPOINTS**
* /webscraper/ASINID : scraps the webpage AND its following ones if any and writes in database the scrapping result. If there is no new content to index, returns HTTP 204 and redirects to /webscraper/ASINID/existing
* /webscraper/ASINID/existing : retrieves all already indexed user reviews for the ASINID

* POST /webscraper ; BODY : applicationtype/json ["ASIN1","ASIN2",...,"ASINN"] : scraps all webpages of the array of asins

I didn't do an endpoint for scrapping the 10 most recent ones, but it is a nice to know that a feature to retrieve the N top exists at least from DB point of view.
And it would need a little bit of adaptation, but not too much, to make that work.

Nice to know : 
- by adding ?sortByRecent=1 to /webscraper/ASINID, it will scrap, and hence display comments by most recent time.
- /webscraper/ASINID/existing already retrieves comments by most recent time thanks to the corresponding SQL request it calls.

Nice to know #2 :
- An SQLite DB will be created at very first startup and all scrapped data will be pushed into it
- I do know where I decided to do not pretty code and can't wait for a discussion with someone to discuss about the possible improvements.
- I have tried to handle all places where I thought my code could fail into exceptions, which, even if some scraping could have been done better,
  is also here to show that I'm aware scraping is something which has to evolve depending on the html code of the website we intend to scrap, should be tracked, and monitored accordingly to take corrective actions.
- There is NO unit test. It's a personal choice which doesn't mean I approve not to write unit tests, at all ! I wanted to focus on the feature first for that tech exam. As you can see, I'm not a TDD developer, but happy to discuss about it. :)


**A NICE END WORD**
I really hope to talk with someone about this exam and the ways I could improve!
Happy deep dive into the code of someone else (I hope you won't be too much like 'w** why is it coded like that?')

