Thank you for taking part in the PURE data challenge.

################################################################################
DISCLAIMER:
The data that has been made available to you concerns an extract of the PURE
database up until the previous REF (2014) submission. Although some quality
checks have been performed to examine the suitability of this data for the PURE
data challenge, absolutely NO guarantees are given concerning the accuracy of
this  data. The data should not be used for any other purpose than to prepare a
submission for the PURE data challenge organised by the Jean Golding Institute
at the University of Bristol from March - May 2017.

Should you wish to use data from PURE to other ends there are a few options
available. You can get in touch with the PURE  technical team to see if they can
provide you with access to the PURE API. Alternatively, if you  can login to
PURE, you can extract the complete PURE database in various file formats from
within PURE itself (e.g. reference manager formats, XML, etc.).
################################################################################

The PURE zip file contains five data files:
outputs.csv
staff.csv
authors.csv
org_hierarchy.csv
org_key.csv

Below you will find a summary of the variables contained in each of these files.
As the aim of the challenge is to see what one can do with the data that can be
exported from PURE, it is good to consider that the data contained in these
files has not been (thoroughly) cleaned yet and you are advised to run your own
cleaning procedures on the data.

outputs.csv
This table contains information on all research output and contains the
following variables:
- publication ID: A unique identifier for each research output
- title
- type of publication code: An integer specific to the type of publication
- type of publication
- publication day
- publication month
- publication year
- abstract
- keywords: All keywords for a publication are included in the same cell, comma
            separated

staff.csv
This table contains information on all staff and contains the following
variables
- person id: A unique identifier for each research output
- published name
- forename
- surname
- organisational code: A code for the school/department the researcher was
                       affiliated with at the point of data export. See
                       org_key.csv for the key to the codes.
- job title

authors.csv
This table links staff to publications and contains the following variables
- person ID: Same identifier as in staff.csv
- publication ID: Same identifier as in outputs.csv
- published name

org_hierarchy.csv
This table provides the hierarchy of organisational units using the organisation
codes
- parent org code
- child org code

org_key.csv
This table can be used as a key to the organisation codes
- organisation code
- organisation type
- full name
- short name
- URL

Good luck with the data challenge!
