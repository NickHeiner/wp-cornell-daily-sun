wp-cornell-daily-sun
====================

Windows Phone newsreader for the Cornell Daily Sun

## API
This app depends on two APIs made available by CornellSun.com. The interaction with these APIs takes place in the [SunData class](https://github.com/NickHeiner/wp-cornell-daily-sun/blob/master/CornellSunNewsreader/Data/SunData.cs). Here they are, in order of use:

### GET sections
**Endpoint**: `/sections.json`
**Result**: 

```json
{
  sections: [
    { name: 'Opinion', vid: 1},
    { name: 'Sports',  vid: 2},   
    // etc
  ]
}
```

Each Section is represented by an instance of the [Section class](https://github.com/NickHeiner/wp-cornell-daily-sun/blob/master/CornellSunNewsreader/Models/Section.cs).

### GET stories
**Endpoint**: `section/wp7/stories/<sectionId>?page=<pageNumber>`
**Result**:

```json
{
  nodes: [
    {
      Body:             'the full text of the story',
      Title:            'The Title of the Story',
      Teaser:           'A quick blurb summarizing the story',
      Nid:               2342, // id for this story; uniquely identifies it amongst all stories
      Vid:               2     // the id of the section that this story belongs to
      field_images_nid: 'http:\/\/cornellsun.com\/files\/images\/Pg-3-tcat---VGao-S.preview.jpg' // a thumbnail url
      Date:             '2 September 2013' // a display-ready string representation of the date
    }
  ]
}
```

Each Story is parsed into an instance of the [StoryJson class](https://github.com/NickHeiner/wp-cornell-daily-sun/blob/master/CornellSunNewsreader/Models/StoryJson.cs).

## History
This app was made by Nick Heiner in Fall 2010 and updated in Fall 2011, 
in cooperation with James Elkins and Rahul Kishore, the respective web editors of the Sun at the time.
It lived in the Cornell Sun svn repo for a long time, until Nick decided to open source it and dump it on github.
For more detail, see the [colophon](https://github.com/NickHeiner/wp-cornell-daily-sun/blob/master/CornellSunNewsreader/Views/Colophon.xaml).
