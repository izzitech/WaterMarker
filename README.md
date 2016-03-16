# WaterMarker!

Please, excuse that this is not even an started poject.
I will be updating what I found about the process.

I'm researching how to do this work.
I will use some parts of Cardiolizer, other repo of mine.
Maybe I should make some kind of shared library.

#This is how it works

## First, convert DOC to ODT
soffice --headless --convert-to odt --outdir documents/ *.dochttps://cgit.freedesktop.org/libreoffice/core/tree/filter/source/config/fragments/filters

## Then insertes this inside de ODT
This at the end of page-layout-properties
<style:background-image xlink:href="Pictures/escopia.png" xlink:type="simple" xlink:actuate="onLoad"/>

And this on the manifest.xml
<manifest:file-entry manifest:media-type="image/png" manifest:full-path="Pictures/escopia.png"/>

