# https://www.tutorialspoint.com/python/python_command_line_arguments.htm
import os, argparse, urllib2
    

def download_osm_pbf(city_name):
    # https://stackoverflow.com/a/22776/178082
    # SGK7 2019-02-22 os.path.join
    directory = os.path.join(os.getcwd(), city_name)
    file_name = os.path.join(directory, city_name + ".osm.pbf")
    url = "https://download.bbbike.org/osm/bbbike/{0}/{0}.osm.pbf".format(city_name)

    print("downloading: " + city_name)
    print("source url: " + url)
    print("local path: " + file_name)
    
    u = urllib2.urlopen(url)
    meta = u.info()
    file_size = int(meta.getheaders("Content-Length")[0])
    print("Downloading: %s Bytes: %s" % (file_name, file_size))

    file_size_dl = 0
    block_sz = 8192
    # SGK7 helped in chat 2019-02-20
    # os.makedirs(directory, exist_ok=True)
    if not os.path.exists(directory):
        os.makedirs(directory)
    with open(file_name, "wb+") as f:
        while True:
            buffer = u.read(block_sz)
            if not buffer:
                break

            file_size_dl += len(buffer)
            f.write(buffer)
            status = r"%10d  [%3.2f%%]" % (file_size_dl, file_size_dl * 100. / file_size)
            status = status + "\r"
            print status,


def build_parser():
    # SGK7 2019-02-20
    # https://github.com/lengstrom/fast-style-transfer/blob/master/style.py
    parser = argparse.ArgumentParser()
    parser.add_argument("-n", "--name", type=str,
                        dest='city_name', help="the name of the city you want to download",
                        metavar="CITY_NAME", required=True)
    return parser


if __name__ == "__main__":
    parser = build_parser()
    options = parser.parse_args()
    download_osm_pbf(options.city_name)