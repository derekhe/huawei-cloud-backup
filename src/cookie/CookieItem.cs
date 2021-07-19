struct CookieItem
{
    public string HostKey;
    public string Name;
    public string Value;

    public override string ToString()
    {
        return $"{HostKey} {Name} {Value}";
    }
}