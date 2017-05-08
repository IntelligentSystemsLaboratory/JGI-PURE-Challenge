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

It is possible to visualise a chord diagram for all the years or the specified
years in a comma-separated list as a GET parameter. Visit the examples:

- To see all the years go to [docs/organizations.html][1]
- To see only 2008 go to [docs/organizations.html?years=2008][2]
- To see both 2012 and 2013 go to [docs/organizations.html?years=2012,2013][3]

[1]:https://perellonieto.github.io/JGI-PURE-Challenge/organizations.html
[2]:https://perellonieto.github.io/JGI-PURE-Challenge/organizations.html?years=2008
[3]:https://perellonieto.github.io/JGI-PURE-Challenge/organizations.html?years=2012,2013
