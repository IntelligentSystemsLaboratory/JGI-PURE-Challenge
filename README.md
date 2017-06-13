# JGI-PURE-Challenge
A competition to identify, analyse and visualise interdisciplinary research at UoB using the PURE dataset.

To run the CSharp solution:

```
git clone git@github.com:IRC-SPHERE/JGI-PURE-Challenge.git
cd JGI-PURE-Challenge/AVPTM
nuget restore
xbuild
(cd bin/Debug && mono AVPTM.exe)
```

# Visualizations

## Common publications between organizations

We counted how many times every research group has collaborated with another
one by counting the number of common publications. With this information we
created a Chord diagram from the years 2008 to 2013 that can be found in
[story/static_chord_diagram.html][1].

It is also possible to query one particular year by passing as a GET argument
the specific year (e.g. for 2008 go to the link
[story/static_chord_diagram.html?years=2008][2]).

The same argument accepts a list of years separated by comma (e.g. for years
2012 and 2013 follow the link [story/static_chord_diagram.html?years=2012,2013][3]).

[1]: https://intelligentsystemslaboratory.github.io/JGI-PURE-Challenge/story/static_chord_diagram.html
[2]: https://intelligentsystemslaboratory.github.io/JGI-PURE-Challenge/story/static_chord_diagram.html?years=2008
[3]: https://intelligentsystemslaboratory.github.io/JGI-PURE-Challenge/story/static_chord_diagram.html?years=2012,2013

We also trained a topic model with the abstracts and titles from the
publications and identified which clusters of researchers and organizations
have worked within each of the discovered topics each year. A visualization of
a scatterplot can be found in [story/static_scatterplot.html][4]

[4]: https://intelligentsystemslaboratory.github.io/JGI-PURE-Challenge/story/static_scatterplot.html

## PURE Challenge slides

You can find the slides for a presentation on this link [intelligentsystemslaboratory.github.io/JGI-PURE-Challenge/story][5]

[5]: https://intelligentsystemslaboratory.github.io/JGI-PURE-Challenge/story/
