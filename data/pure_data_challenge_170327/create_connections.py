import pandas as pd
from collections import Counter

csv_files = ['outputs', 'authors', 'staff']

dfs = {name:pd.read_csv(name + '.csv', infer_datetime_format=True,
                        index_col=False) for name in csv_files}

keep_columns = {'outputs': ['PUBLICATION_ID', 'PUBLICATION_YEAR'],
                'authors': ['PERSON_ID', 'PUBLICATION_ID'],
                'staff': ['PERSON_ID', 'ORGANISATION_CODE']}

dfs = {name:dfs[name][columns] for name, columns in keep_columns.items()}

for name, df in dfs.items():
    print('{} shape = {}'.format(name, df.shape))


# We get: person_id, publication_id, and organization_code
df = pd.merge(dfs['authors'], dfs['staff'], on='PERSON_ID')
# We get: person_id, publication_id, and organization_code, publication_year
df = pd.merge(df, dfs['outputs'], on='PUBLICATION_ID')
# We get: publication_id, and organization_code, publication_year
del df['PERSON_ID']

# We get: publication_id, list of organisation_code
df_pub_org = df.groupby(['PUBLICATION_ID'])['ORGANISATION_CODE'].unique()

counter =  Counter(tuple(sorted(tup)) for tup in df_pub_org.values)

connections = []
for tup, counts in counter.items():
    if len(tup) == 1:
        org1 = org2 = tup[0]
        connections.append([2012, org1, org2, counts, counts])
    else:
        # FIXME Solve problem with duplicated pairs
        for i in range(len(tup)):
            for j in range(i,len(tup)):
                org1 = tup[i]
                org2 = tup[j]
                connections.append([2012, org1, org2, counts, counts])

df_connections = pd.DataFrame(connections, columns=['year', 'importer1',
                                                    'importer2', 'flow1',
                                                    'flow2'])
df_connections.to_csv('org_connections.csv', index=False)
