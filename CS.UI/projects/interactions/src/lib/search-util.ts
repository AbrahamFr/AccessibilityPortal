export function escapeRegExp(str: string) {
  return str.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"); // $& means the whole matched string
}

export function isSearchMatch(
  stringToSearch: string,
  searchTerm: string
): Boolean {
  return (
    stringToSearch.toLowerCase().search(searchTerm.trim().toLowerCase()) > -1
  );
}
